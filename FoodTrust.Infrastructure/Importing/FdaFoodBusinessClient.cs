using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FoodTrust.Core.RestaurantImports.Interfaces;
using FoodTrust.Core.RestaurantImports.Models;
using FoodTrust.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoodTrust.Infrastructure.Importing;

public sealed class FdaFoodBusinessClient(
    HttpClient httpClient,
    IOptions<RestaurantImportOptions> options,
    ILogger<FdaFoodBusinessClient> logger) : IRestaurantImportSource
{
    private const int MaxRedirects = 5;

    public string SourceSystem => "TaiwanFdaFoodBusiness";

    public string SourceUrl => options.Value.SourceUrl;

    /// <summary>
    /// 從台灣 FDA 食品業者資料來源下載並解析候選餐廳資料。
    /// </summary>
    public async Task<IReadOnlyList<RestaurantImportRecord>> FetchRestaurantsAsync(CancellationToken cancellationToken)
    {
        using var response = await GetWithRedirectsAsync(options.Value.SourceUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var text = await DecodeContentAsync(content, cancellationToken);
        var records = ParseContent(text, options.Value)
            .Take(Math.Max(1, options.Value.MaxRecords))
            .ToArray();
        if (records.Length == 0)
        {
            logger.LogWarning(
                "FDA source returned no importable records. Response preview: {ResponsePreview}",
                BuildPreview(text));
        }

        logger.LogInformation("Fetched {Count} candidate records from Taiwan FDA food business source.", records.Length);

        return records;
    }

    private async Task<HttpResponseMessage> GetWithRedirectsAsync(
        string sourceUrl,
        CancellationToken cancellationToken)
    {
        var requestUri = new Uri(sourceUrl, UriKind.Absolute);

        for (var redirectCount = 0; redirectCount <= MaxRedirects; redirectCount++)
        {
            var response = await httpClient.GetAsync(requestUri, cancellationToken);
            if (!IsRedirect(response.StatusCode))
            {
                return response;
            }

            var statusCode = response.StatusCode;
            var location = response.Headers.Location;
            response.Dispose();

            if (location is null)
            {
                throw new HttpRequestException(
                    $"FDA source returned {(int)statusCode} without a Location header.");
            }

            requestUri = location.IsAbsoluteUri
                ? location
                : new Uri(requestUri, location);

            logger.LogInformation(
                "Following FDA source redirect {RedirectCount}: {RedirectUri}",
                redirectCount + 1,
                requestUri);
        }

        throw new HttpRequestException(
            $"FDA source exceeded the maximum redirect count of {MaxRedirects}.");
    }

    private static bool IsRedirect(HttpStatusCode statusCode)
    {
        return statusCode is
            HttpStatusCode.MovedPermanently or
            HttpStatusCode.Found or
            HttpStatusCode.SeeOther or
            HttpStatusCode.TemporaryRedirect or
            HttpStatusCode.PermanentRedirect;
    }

    /// <summary>
    /// 解碼下載內容，包含 ZIP 壓縮資料。
    /// </summary>
    private static async Task<string> DecodeContentAsync(byte[] content, CancellationToken cancellationToken)
    {
        if (content.Length >= 4 && content[0] == 0x50 && content[1] == 0x4B)
        {
            await using var zipStream = new MemoryStream(content);
            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
            var entry = archive.Entries.FirstOrDefault(e => e.Length > 0)
                ?? throw new InvalidOperationException("The downloaded ZIP file did not contain any data file.");

            await using var entryStream = entry.Open();
            using var reader = new StreamReader(entryStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            return await reader.ReadToEndAsync(cancellationToken);
        }

        return Encoding.UTF8.GetString(content);
    }

    /// <summary>
    /// 判斷來源格式並解析 JSON 或 CSV 內容。
    /// </summary>
    private static IReadOnlyList<RestaurantImportRecord> ParseContent(
        string content,
        RestaurantImportOptions options)
    {
        var trimmed = content.TrimStart('\uFEFF', ' ', '\t', '\r', '\n');
        return trimmed.StartsWith('[') || trimmed.StartsWith('{')
            ? ParseJson(trimmed, options)
            : ParseCsv(trimmed, options);
    }

    /// <summary>
    /// 將 FDA JSON 陣列內容解析為匯入資料。
    /// </summary>
    private static IReadOnlyList<RestaurantImportRecord> ParseJson(
        string json,
        RestaurantImportOptions options)
    {
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Taiwan FDA food business response must be a JSON array.");
        }

        var records = new List<RestaurantImportRecord>();
        var elements = document.RootElement.EnumerateArray().ToArray();

        if (TryParseJsonObjectPairsRows(elements, records, options) ||
            TryParseJsonArrayRows(elements, records, options))
        {
            return records;
        }

        foreach (var element in elements)
        {
            AddRecord(records, element, options);
        }

        return records;
    }

    private static bool TryParseJsonObjectPairsRows(
        IReadOnlyList<JsonElement> elements,
        ICollection<RestaurantImportRecord> records,
        RestaurantImportOptions options)
    {
        if (elements.Count == 0 ||
            elements[0].ValueKind != JsonValueKind.Array ||
            !elements[0].EnumerateArray().Any(item => item.ValueKind == JsonValueKind.Object))
        {
            return false;
        }

        foreach (var element in elements)
        {
            if (element.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            var row = new Dictionary<string, string?>(StringComparer.Ordinal);
            foreach (var item in element.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                foreach (var property in item.EnumerateObject())
                {
                    row[NormalizeHeader(property.Name)] =
                        property.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
                            ? null
                            : property.Value.ToString();
                }
            }

            var name = GetString(row, Field.BusinessNameAliases);
            var address = GetString(row, Field.AddressAliases);
            var registrationNo = GetString(row, Field.RegistrationNoAliases);
            var businessItem = GetString(row, Field.BusinessItemAliases);
            var phoneNumber = GetString(row, Field.PhoneAliases);

            AddRecord(records, name, address, registrationNo, businessItem, phoneNumber, element.GetRawText(), options);
        }

        return true;
    }

    private static bool TryParseJsonArrayRows(
        IReadOnlyList<JsonElement> elements,
        ICollection<RestaurantImportRecord> records,
        RestaurantImportOptions options)
    {
        if (elements.Count == 0 ||
            elements[0].ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        var headers = elements[0]
            .EnumerateArray()
            .Select(value => NormalizeHeader(value.ToString()))
            .ToArray();

        if (!ContainsKnownHeader(headers))
        {
            return false;
        }

        foreach (var element in elements.Skip(1))
        {
            if (element.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            var values = element
                .EnumerateArray()
                .Select(value => value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
                    ? null
                    : value.ToString())
                .ToArray();

            var row = headers
                .Select((header, index) => new
                {
                    Header = header,
                    Value = index < values.Length ? values[index] : null
                })
                .Where(item => !string.IsNullOrWhiteSpace(item.Header))
                .ToDictionary(item => item.Header, item => item.Value);

            var name = GetString(row, Field.BusinessNameAliases);
            var address = GetString(row, Field.AddressAliases);
            var registrationNo = GetString(row, Field.RegistrationNoAliases);
            var businessItem = GetString(row, Field.BusinessItemAliases);
            var phoneNumber = GetString(row, Field.PhoneAliases);

            AddRecord(records, name, address, registrationNo, businessItem, phoneNumber, element.GetRawText(), options);
        }

        return true;
    }

    private static bool ContainsKnownHeader(IEnumerable<string> headers)
    {
        var headerSet = headers.ToHashSet(StringComparer.Ordinal);
        return Field.BusinessNameAliases.Any(headerSet.Contains) ||
            Field.AddressAliases.Any(headerSet.Contains);
    }

    /// <summary>
    /// 將 FDA CSV 內容解析為匯入資料。
    /// </summary>
    private static IReadOnlyList<RestaurantImportRecord> ParseCsv(
        string csv,
        RestaurantImportOptions options)
    {
        using var reader = new StringReader(csv);
        var headerLine = reader.ReadLine();
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            return [];
        }

        var headers = ParseCsvLine(headerLine).Select(NormalizeHeader).ToArray();
        var records = new List<RestaurantImportRecord>();

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var values = ParseCsvLine(line);
            var row = headers
                .Select((header, index) => new
                {
                    Header = header,
                    Value = index < values.Count ? values[index] : null
                })
                .Where(item => !string.IsNullOrWhiteSpace(item.Header))
                .ToDictionary(item => item.Header, item => item.Value);

            var name = GetString(row, Field.BusinessNameAliases);
            var address = GetString(row, Field.AddressAliases);
            var registrationNo = GetString(row, Field.RegistrationNoAliases);
            var businessItem = GetString(row, Field.BusinessItemAliases);
            var phoneNumber = GetString(row, Field.PhoneAliases);

            AddRecord(records, name, address, registrationNo, businessItem, phoneNumber, line, options);
        }

        return records;
    }

    /// <summary>
    /// 當資料列符合餐飲業者條件時，加入正規化後的匯入資料。
    /// </summary>
    private static void AddRecord(
        ICollection<RestaurantImportRecord> records,
        JsonElement element,
        RestaurantImportOptions options)
    {
        var name = GetString(element, Field.BusinessNameAliases);
        var address = GetString(element, Field.AddressAliases);
        var registrationNo = GetString(element, Field.RegistrationNoAliases);
        var businessItem = GetString(element, Field.BusinessItemAliases);
        var phoneNumber = GetString(element, Field.PhoneAliases);

        AddRecord(records, name, address, registrationNo, businessItem, phoneNumber, element.GetRawText(), options);
    }

    private static void AddRecord(
        ICollection<RestaurantImportRecord> records,
        string? name,
        string? address,
        string? registrationNo,
        string? businessItem,
        string? phoneNumber,
        string rawPayload,
        RestaurantImportOptions options)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(address))
        {
            return;
        }

        if (!IsRestaurantBusiness(businessItem, name))
        {
            return;
        }

        if (!IsAllowedAddress(address, options.AddressPrefixes))
        {
            return;
        }

        var sourceKey = !string.IsNullOrWhiteSpace(registrationNo)
            ? registrationNo.Trim()
            : BuildStableKey(name, address);

        records.Add(new RestaurantImportRecord(
            "TaiwanFdaFoodBusiness",
            sourceKey,
            NormalizeWhitespace(name),
            NormalizeWhitespace(address),
            string.IsNullOrWhiteSpace(phoneNumber) ? null : NormalizeWhitespace(phoneNumber),
            rawPayload));
    }

    /// <summary>
    /// 從 JSON 元素取得第一個符合的字串屬性。
    /// </summary>
    private static string? GetString(JsonElement element, params string[] propertyNames)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        foreach (var propertyName in propertyNames)
        {
            if (element.TryGetProperty(propertyName, out var value) &&
                value.ValueKind != JsonValueKind.Null &&
                value.ValueKind != JsonValueKind.Undefined)
            {
                return value.ToString();
            }
        }

        return null;
    }

    /// <summary>
    /// 從 CSV 資料列字典取得第一個符合的字串值。
    /// </summary>
    private static string? GetString(IReadOnlyDictionary<string, string?> row, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (row.TryGetValue(NormalizeHeader(propertyName), out var value))
            {
                return value;
            }
        }

        return null;
    }

    /// <summary>
    /// 解析單行 CSV，並支援引號包住的欄位值。
    /// </summary>
    private static List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var value = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var current = line[i];
            if (current == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    value.Append('"');
                    i++;
                    continue;
                }

                inQuotes = !inQuotes;
                continue;
            }

            if (current == ',' && !inQuotes)
            {
                values.Add(value.ToString());
                value.Clear();
                continue;
            }

            value.Append(current);
        }

        values.Add(value.ToString());
        return values;
    }

    /// <summary>
    /// 正規化 CSV 標題名稱以便字典查找。
    /// </summary>
    private static string NormalizeHeader(string value)
    {
        return value.Trim().TrimStart('\uFEFF');
    }

    /// <summary>
    /// 判斷資料列是否看起來代表餐廳或餐飲業者。
    /// </summary>
    private static bool IsRestaurantBusiness(string? businessItem, string name)
    {
        if (!string.IsNullOrWhiteSpace(businessItem) &&
            businessItem.Contains(Field.RestaurantIndustry, StringComparison.Ordinal))
        {
            return true;
        }

        return Field.RestaurantKeywords.Any(keyword => name.Contains(keyword, StringComparison.Ordinal));
    }

    private static bool IsAllowedAddress(string address, IReadOnlyCollection<string> addressPrefixes)
    {
        if (addressPrefixes.Count == 0)
        {
            return true;
        }

        var normalizedAddress = NormalizeWhitespace(address);
        return addressPrefixes.Any(prefix =>
            !string.IsNullOrWhiteSpace(prefix) &&
            normalizedAddress.StartsWith(prefix.Trim(), StringComparison.Ordinal));
    }

    /// <summary>
    /// 合併來源值中的重複空白字元。
    /// </summary>
    private static string NormalizeWhitespace(string value)
    {
        return string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    private static string BuildPreview(string content)
    {
        var normalized = NormalizeWhitespace(content);
        return normalized.Length <= 1000
            ? normalized
            : normalized[..1000];
    }

    /// <summary>
    /// 當外部資料缺少登錄字號時，建立穩定的來源鍵。
    /// </summary>
    private static string BuildStableKey(string name, string address)
    {
        var normalized = $"{NormalizeWhitespace(name)}|{NormalizeWhitespace(address)}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(hash);
    }

    private static class Field
    {
        public const string BusinessName = "\u516c\u53f8\u6216\u5546\u696d\u767b\u8a18\u540d\u7a31";
        public const string CompanyName = "\u516c\u53f8\u540d\u7a31";
        public const string RegisteredBusinessName = "\u5546\u696d\u767b\u8a18\u540d\u7a31";
        public const string FoodBusinessName = "\u98df\u54c1\u696d\u8005\u540d\u7a31";
        public const string FoodBusinessRegisteredName = "\u98df\u54c1\u696d\u8005\u767b\u9304\u540d\u7a31";
        public const string OperatorName = "\u696d\u8005\u540d\u7a31";
        public const string BusinessAddress = "\u696d\u8005\u5730\u5740";
        public const string FoodBusinessAddress = "\u98df\u54c1\u696d\u8005\u5730\u5740";
        public const string FoodBusinessRegisteredAddress = "\u98df\u54c1\u696d\u8005\u767b\u9304\u5730\u5740";
        public const string Address = "\u5730\u5740";
        public const string FoodBusinessRegistrationNo = "\u98df\u54c1\u696d\u8005\u767b\u9304\u5b57\u865f";
        public const string RegistrationNo = "\u767b\u9304\u5b57\u865f";
        public const string RegistrationItem = "\u767b\u9304\u9805\u76ee";
        public const string FoodBusinessRegistrationItem = "\u98df\u54c1\u696d\u8005\u767b\u9304\u9805\u76ee";
        public const string BusinessItem = "\u71df\u696d\u9805\u76ee";
        public const string Phone = "\u96fb\u8a71";
        public const string ContactPhone = "\u9023\u7d61\u96fb\u8a71";
        public const string FoodBusinessPhone = "\u98df\u54c1\u696d\u8005\u96fb\u8a71";
        public const string RestaurantIndustry = "\u9910\u98f2";

        public static readonly string[] BusinessNameAliases =
        [
            BusinessName,
            CompanyName,
            RegisteredBusinessName,
            FoodBusinessName,
            FoodBusinessRegisteredName,
            OperatorName,
            "Name"
        ];

        public static readonly string[] AddressAliases =
        [
            BusinessAddress,
            FoodBusinessAddress,
            FoodBusinessRegisteredAddress,
            Address,
            "Address"
        ];

        public static readonly string[] RegistrationNoAliases =
        [
            FoodBusinessRegistrationNo,
            RegistrationNo,
            "RegisterNo"
        ];

        public static readonly string[] BusinessItemAliases =
        [
            RegistrationItem,
            FoodBusinessRegistrationItem,
            BusinessItem,
            "BusinessItem"
        ];

        public static readonly string[] PhoneAliases =
        [
            Phone,
            ContactPhone,
            FoodBusinessPhone,
            "Phone",
            "PhoneNumber"
        ];

        public static readonly string[] RestaurantKeywords =
        [
            "\u9910\u5ef3",
            "\u98ef\u5e97",
            "\u5c0f\u5403",
            "\u4fbf\u7576",
            "\u706b\u934b",
            "\u71d2\u8089",
            "\u71d2\u70e4",
            "\u62c9\u9eb5",
            "\u9eb5\u5e97",
            "\u5496\u5561",
            "\u65e9\u5348\u9910",
            "\u65e9\u9910",
            "\u6ef7\u5473",
            "\u9e7d\u9165\u96de",
            "\u725b\u6392",
            "\u58fd\u53f8",
            "\u5c45\u9152\u5c4b"
        ];
    }
}
