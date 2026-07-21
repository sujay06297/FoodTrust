namespace FoodTrust.Core.Restaurants.Domain.ValueObjects;

public sealed record PriceRange
{
    private PriceRange(int? minimum, int? maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    public int? Minimum { get; }

    public int? Maximum { get; }

    public static PriceRange Create(int? minimum, int? maximum)
    {
        if (minimum is < 0 || maximum is < 0)
        {
            throw new ArgumentException("Restaurant price cannot be negative.");
        }

        if (minimum is not null && maximum is not null && minimum > maximum)
        {
            throw new ArgumentException("Restaurant price minimum cannot be greater than price maximum.");
        }

        return new PriceRange(minimum, maximum);
    }
}
