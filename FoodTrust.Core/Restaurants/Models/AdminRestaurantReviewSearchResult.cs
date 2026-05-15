namespace FoodTrust.Core.Restaurants.Models;

public sealed record AdminRestaurantReviewSearchResult(
    IReadOnlyList<AdminRestaurantReviewListItem> Items,
    int Page,
    int PageSize,
    long TotalCount);

