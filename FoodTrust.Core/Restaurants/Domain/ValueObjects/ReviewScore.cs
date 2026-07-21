namespace FoodTrust.Core.Restaurants.Domain.ValueObjects;

public sealed record ReviewScore
{
    private ReviewScore(decimal value)
    {
        Value = value;
    }

    public decimal Value { get; }

    public static ReviewScore Create(decimal value, string name)
    {
        if (value is < 1m or > 5m)
        {
            throw new ArgumentException("Restaurant review score must be between 1 and 5.", name);
        }

        return new ReviewScore(value);
    }
}
