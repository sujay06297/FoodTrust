namespace FoodTrust.Core.Admin.Domain.ValueObjects;

public sealed record AdminDisplayName
{
    private AdminDisplayName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static AdminDisplayName Create(string? value, AdminUsername? fallbackUsername)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? fallbackUsername?.Value ?? "admin" : value.Trim();
        if (normalized.Length is < 2 or > 100)
        {
            throw new ArgumentException("Admin display name length is invalid.", nameof(value));
        }

        return new AdminDisplayName(normalized);
    }
}
