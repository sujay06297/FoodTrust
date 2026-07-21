using FoodTrust.Core.Admin.Models;

namespace FoodTrust.Core.Admin.Domain.ValueObjects;

public sealed record AdminRoleName
{
    private AdminRoleName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static AdminRoleName Create(string? value)
    {
        var normalized = value?.Trim();
        if (!AdminRole.IsValid(normalized))
        {
            throw new ArgumentException("Invalid admin role.", nameof(value));
        }

        return new AdminRoleName(normalized!);
    }
}
