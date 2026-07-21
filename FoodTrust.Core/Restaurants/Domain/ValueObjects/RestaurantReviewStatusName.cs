using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Core.Restaurants.Domain.ValueObjects;

public sealed record RestaurantReviewStatusName
{
    private RestaurantReviewStatusName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static RestaurantReviewStatusName Create(string? value)
    {
        var normalized = value?.Trim();
        if (!RestaurantReviewStatus.IsValid(normalized))
        {
            throw new ArgumentException("Invalid restaurant review status.", nameof(value));
        }

        return new RestaurantReviewStatusName(normalized!);
    }
}
