namespace FoodTrust.Core.Admin.Models;

public sealed record AdminRefreshToken(
    long Id,
    long AdminUserId,
    string TokenHash,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt,
    DateTimeOffset CreatedAt);
