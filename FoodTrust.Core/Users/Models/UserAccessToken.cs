namespace FoodTrust.Core.Users.Models;

public sealed record UserAccessToken(
    string Token,
    DateTimeOffset ExpiresAt);
