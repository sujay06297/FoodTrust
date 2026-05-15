namespace FoodTrust.Core.Users.Models;

public sealed record UserAuthResult(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    UserSummary User);
