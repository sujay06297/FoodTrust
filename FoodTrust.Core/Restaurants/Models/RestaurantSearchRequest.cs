namespace FoodTrust.Core.Restaurants.Models;

public sealed record RestaurantSearchRequest(
    string? Keyword,
    string? Status,
    string? City,
    string? District,
    string? CuisineType,
    int? PriceMin,
    int? PriceMax,
    decimal? MinScore,
    string? SortBy,
    int Page,
    int PageSize);
