namespace FoodTrust.Infrastructure.Options;

public sealed class RestaurantImportOptions
{
    public const string SectionName = "RestaurantImport";

    public string ConnectionString { get; init; } = "Server=localhost;Port=3306;Database=foodtrust;User ID=root;Password=;CharSet=utf8mb4;Allow User Variables=True;";

    public bool EnsureDatabase { get; init; } = true;

    public string? BootstrapDatabase { get; init; }

    public string? CaCertificateFile { get; init; }

    public string? TlsVersion { get; init; }

    public string SourceUrl { get; init; } = "https://data.fda.gov.tw/opendata/exportDataList.do?method=ExportData&InfoId=97&logType=3";

    public int IntervalHours { get; init; } = 24;

    public bool RunOnStartup { get; init; } = true;

    public int BatchSize { get; init; } = 500;

    public int MaxRecords { get; init; } = 5000;

    public string[] AddressPrefixes { get; init; } = [];
}
