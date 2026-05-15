namespace FoodTrust.Core.Restaurants.Models;

public sealed record RestaurantSearchRequest(
    string? Keyword,
    string? Status,
    string? City,
    string? District,
    string? CuisineType,
    int? PriceMin,
    int? PriceMax,
    int Page,
    int PageSize);
