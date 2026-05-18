namespace FoodTrust.Core.Restaurants.Models;

public sealed record FavoriteRestaurantSearchResult(
    IReadOnlyList<FavoriteRestaurantListItem> Items,
    int Page,
    int PageSize,
    long TotalCount);
