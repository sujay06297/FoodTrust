namespace FoodTrust.Core.Users.Models;

public sealed record UserSummary(
    long Id,
    string Email,
    string DisplayName,
    string Status,
    DateTimeOffset CreatedAt);
