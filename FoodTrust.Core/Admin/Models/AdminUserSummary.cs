namespace FoodTrust.Core.Admin.Models;

public sealed record AdminUserSummary(
    long Id,
    string Username,
    string DisplayName,
    string Role,
    bool IsActive,
    DateTimeOffset CreatedAt);
