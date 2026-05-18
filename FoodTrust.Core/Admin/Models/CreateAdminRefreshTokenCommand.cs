namespace FoodTrust.Core.Admin.Models;

public sealed record CreateAdminRefreshTokenCommand(
    long AdminUserId,
    string TokenHash,
    DateTimeOffset ExpiresAt);
