namespace FoodTrust.Core.Users.Models;

public sealed record User(
    long Id,
    string Email,
    string PasswordHash,
    string DisplayName,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
