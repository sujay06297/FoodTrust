namespace FoodTrust.Core.Restaurants.Models;

public sealed record RestaurantRankingItem(
    long Id,
    string Name,
    string Address,
    string? PhoneNumber,
    double AverageScore,
    int RatingCount);
