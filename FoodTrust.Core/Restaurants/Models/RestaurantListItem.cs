namespace FoodTrust.Core.Restaurants.Models;

public sealed record RestaurantListItem(
    long Id,
    string Name,
    string Address,
    string? PhoneNumber,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
