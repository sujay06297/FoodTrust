namespace FoodTrust.Core.Users.Domain.ValueObjects;

public sealed record DisplayName
{
    private DisplayName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static DisplayName Create(string? value, UserEmail fallbackEmail)
    {
        var normalized = string.IsNullOrWhiteSpace(value)
            ? fallbackEmail.Value.Split('@')[0]
            : value.Trim();

        if (normalized.Length is < 2 or > 100)
        {
            throw new ArgumentException("User display name length is invalid.", nameof(value));
        }

        return new DisplayName(normalized);
    }
}
