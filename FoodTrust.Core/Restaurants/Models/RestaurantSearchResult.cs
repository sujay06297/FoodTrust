namespace FoodTrust.Core.Restaurants.Models;

public sealed record RestaurantSearchResult(
    IReadOnlyList<RestaurantListItem> Items,
    int Page,
    int PageSize,
    long TotalCount);
