namespace FoodTrust.Core.Admin.Domain.ValueObjects;

public sealed record AdminUsername
{
    private AdminUsername(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static AdminUsername Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Invalid username.", nameof(value));
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length is < 3 or > 100)
        {
            throw new ArgumentException("Invalid username.", nameof(value));
        }

        return new AdminUsername(normalized);
    }

    public static string? NormalizeForLogin(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
    }
}
