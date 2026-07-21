namespace FoodTrust.Core.Restaurants.Domain.ValueObjects;

public sealed record RestaurantAddress
{
    public const int MaxLength = 500;

    private RestaurantAddress(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static RestaurantAddress Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Restaurant address is required.", nameof(value));
        }

        var normalized = value.Trim();
        if (normalized.Length > MaxLength)
        {
            throw new ArgumentException($"Restaurant address cannot exceed {MaxLength} characters.", nameof(value));
        }

        return new RestaurantAddress(normalized);
    }

    public override string ToString()
    {
        return Value;
    }
}
