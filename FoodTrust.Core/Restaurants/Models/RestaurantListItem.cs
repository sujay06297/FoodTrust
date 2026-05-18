namespace FoodTrust.Core.Restaurants.Models;

public sealed record RestaurantListItem(
    long Id,
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
    int FavoriteCount,
    int ReviewCount,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
