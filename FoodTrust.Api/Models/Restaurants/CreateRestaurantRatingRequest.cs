namespace FoodTrust.Api.Models.Restaurants;

public sealed record CreateRestaurantRatingRequest(
    int Score,
    string? Comment,
    string? ReviewerName);
