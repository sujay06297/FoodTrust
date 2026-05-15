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
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
