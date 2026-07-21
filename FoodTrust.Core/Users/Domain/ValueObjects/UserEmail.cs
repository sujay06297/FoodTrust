using System.Text.RegularExpressions;

namespace FoodTrust.Core.Users.Domain.ValueObjects;

public sealed partial record UserEmail
{
    private UserEmail(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static UserEmail Create(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
        if (!EmailRegex().IsMatch(normalized))
        {
            throw new ArgumentException("Invalid user email.", nameof(value));
        }

        return new UserEmail(normalized);
    }

    public static string NormalizeForLogin(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }

    [GeneratedRegex("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();
}
