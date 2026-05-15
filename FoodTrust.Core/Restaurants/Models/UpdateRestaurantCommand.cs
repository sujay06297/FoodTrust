namespace FoodTrust.Core.Restaurants.Models;

public sealed record UpdateRestaurantCommand(
    string Name,
    string Address,
    string? PhoneNumber,
    string? BranchName,
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
    string? GoogleMapUrl);
