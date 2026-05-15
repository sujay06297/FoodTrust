namespace FoodTrust.Core.Restaurants.Models;

public sealed record RestaurantRankingItem(
    long Id,
    string Name,
    string Address,
    string? PhoneNumber,
    decimal RawAverageScore,
    decimal PlatformScore,
    decimal RankingScore,
    int ReviewCount);
