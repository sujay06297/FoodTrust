namespace FoodTrust.Core.Restaurants.Models;

public sealed record RestaurantDetail(
    long Id,
    string Name,
    string? BranchName,
    string Address,
    string? PhoneNumber,
    string? City,
    string? District,
    decimal? Latitude,
    decimal? Longitude,
    string? OpeningHours,
    int? PriceMin,
    int? PriceMax,
    string? CuisineType,
    string? Tags,
    string? Description,
    string? OfficialUrl,
    string? GoogleMapUrl,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<RestaurantSourceDetail> Sources);
