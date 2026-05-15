namespace FoodTrust.Core.Restaurants.Models;

public sealed record RestaurantSearchRequest(
    string? Keyword,
    string? Status,
    int Page,
    int PageSize);
