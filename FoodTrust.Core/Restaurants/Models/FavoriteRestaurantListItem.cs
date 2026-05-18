namespace FoodTrust.Core.Restaurants.Models;

public sealed record FavoriteRestaurantListItem(
    long RestaurantId,
    string Name,
    string? BranchName,
    string Address,
    string? PhoneNumber,
    string? City,
    string? District,
    int? PriceMin,
    int? PriceMax,
    string? CuisineType,
    decimal? RawAverageScore,
    decimal? PlatformScore,
    int ReviewCount,
    string Status,
    DateTimeOffset FavoritedAt);
