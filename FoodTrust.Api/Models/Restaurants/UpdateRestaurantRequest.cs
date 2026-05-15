namespace FoodTrust.Api.Models.Restaurants;

public sealed record UpdateRestaurantRequest(
    string Name,
    string Address,
    string? PhoneNumber);
