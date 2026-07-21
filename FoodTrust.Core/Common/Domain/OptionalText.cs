namespace FoodTrust.Core.Common.Domain;

public sealed record OptionalText
{
    private OptionalText(string? value)
    {
        Value = value;
    }

    public string? Value { get; }

    public static OptionalText Create(string? value, int? maxLength = null, string? name = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new OptionalText((string?)null);
        }

        var normalized = value.Trim();
        if (maxLength is not null && normalized.Length > maxLength)
        {
            throw new ArgumentException($"{name ?? "Text"} cannot exceed {maxLength} characters.", name ?? nameof(value));
        }

        return new OptionalText(normalized);
    }
}
