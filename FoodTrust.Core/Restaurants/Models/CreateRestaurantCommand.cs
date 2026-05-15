namespace FoodTrust.Core.Restaurants.Models;

public sealed record CreateRestaurantCommand(
    string Name,
    string Address,
    string? PhoneNumber);
