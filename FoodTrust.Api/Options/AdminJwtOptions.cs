namespace FoodTrust.Api.Options;

public sealed class AdminJwtOptions
{
    public const string SectionName = "AdminJwt";

    public string Issuer { get; init; } = "FoodTrust";

    public string Audience { get; init; } = "FoodTrust.Admin";

    public string SigningKey { get; init; } = string.Empty;

    public int ExpirationMinutes { get; init; } = 120;
}
