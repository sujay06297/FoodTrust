namespace FoodTrust.Api.Options;

public sealed class UserJwtOptions
{
    public const string SectionName = "UserJwt";

    public string Issuer { get; init; } = "FoodTrust";

    public string Audience { get; init; } = "FoodTrust.User";

    public string SigningKey { get; init; } = string.Empty;

    public int ExpirationMinutes { get; init; } = 120;
}
