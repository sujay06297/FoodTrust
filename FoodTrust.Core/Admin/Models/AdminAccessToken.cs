namespace FoodTrust.Core.Admin.Models;

public sealed record AdminAccessToken(
    string Token,
    DateTimeOffset ExpiresAt);
