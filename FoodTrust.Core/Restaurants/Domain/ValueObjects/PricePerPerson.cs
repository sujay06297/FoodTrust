namespace FoodTrust.Core.Restaurants.Domain.ValueObjects;

public sealed record PricePerPerson
{
    private PricePerPerson(int? value)
    {
        Value = value;
    }

    public int? Value { get; }

    public static PricePerPerson Create(int? value)
    {
        if (value is < 0)
        {
            throw new ArgumentException("Price per person cannot be negative.", nameof(value));
        }

        return new PricePerPerson(value);
    }
}
