namespace FoodTrust.Api.Models.Restaurants;

public sealed record CreateRestaurantRequest(
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
