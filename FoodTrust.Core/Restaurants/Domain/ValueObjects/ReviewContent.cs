namespace FoodTrust.Core.Restaurants.Domain.ValueObjects;

public sealed record ReviewContent
{
    private ReviewContent(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ReviewContent Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length < 30)
        {
            throw new ArgumentException("Restaurant review content must be at least 30 characters.", nameof(value));
        }

        return new ReviewContent(value.Trim());
    }
}
