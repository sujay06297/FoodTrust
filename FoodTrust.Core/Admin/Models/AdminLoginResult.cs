namespace FoodTrust.Core.Admin.Models;

public sealed record AdminLoginResult(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    AdminUserSummary User);
