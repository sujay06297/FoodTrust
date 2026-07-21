namespace FoodTrust.Core.Users.Domain.ValueObjects;

public sealed record AccountPassword
{
    public const int MinimumLength = 12;

    private AccountPassword(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static AccountPassword Create(string? value, string name = "password")
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < MinimumLength)
        {
            throw new ArgumentException("Password must be at least 12 characters.", name);
        }

        return new AccountPassword(value);
    }

    public void EnsureDifferentFrom(AccountPassword other)
    {
        if (Value == other.Value)
        {
            throw new ArgumentException("New password must be different from current password.", nameof(other));
        }
    }
}
