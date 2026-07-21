namespace FoodTrust.Core.Restaurants.Domain.ValueObjects;

public sealed record RestaurantName
{
    public const int MaxLength = 200;

    private RestaurantName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static RestaurantName Create(string? value)
    {
        var normalized = NormalizeRequired(value, "Restaurant name is required.");

        if (normalized.Length > MaxLength)
        {
            throw new ArgumentException($"Restaurant name cannot exceed {MaxLength} characters.", nameof(value));
        }

        return new RestaurantName(normalized);
    }

    public override string ToString()
    {
        return Value;
    }

    private static string NormalizeRequired(string? value, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(errorMessage, nameof(value));
        }

        return value.Trim();
    }
}
