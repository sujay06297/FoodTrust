namespace FoodTrust.Core.Admin.Models;

public sealed record AdminUser(
    long Id,
    string Username,
    string PasswordHash,
    string DisplayName,
    string Role,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
