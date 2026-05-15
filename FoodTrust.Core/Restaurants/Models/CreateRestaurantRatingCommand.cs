namespace FoodTrust.Core.Restaurants.Models;

public sealed record CreateRestaurantRatingCommand(
    int Score,
    string? Comment,
    string? ReviewerName);
