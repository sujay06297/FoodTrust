namespace FoodTrust.Core.Restaurants.Models;

public sealed record UpdateRestaurantCommand(
    string Name,
    string Address,
    string? PhoneNumber);
