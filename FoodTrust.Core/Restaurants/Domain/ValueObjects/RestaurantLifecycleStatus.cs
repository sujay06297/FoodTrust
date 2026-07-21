using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Core.Restaurants.Domain.ValueObjects;

public sealed record RestaurantLifecycleStatus
{
    private RestaurantLifecycleStatus(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static RestaurantLifecycleStatus Create(string? value)
    {
        var normalized = value?.Trim();
        if (!RestaurantStatus.IsValid(normalized))
        {
            throw new ArgumentException("Invalid restaurant status.", nameof(value));
        }

        return new RestaurantLifecycleStatus(normalized!);
    }

    public override string ToString()
    {
        return Value;
    }
}
