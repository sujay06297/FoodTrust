namespace FoodTrust.Api.Models.Restaurants;

public sealed record CreateRestaurantRequest(
    string Name,
    string Address,
    string? PhoneNumber);
