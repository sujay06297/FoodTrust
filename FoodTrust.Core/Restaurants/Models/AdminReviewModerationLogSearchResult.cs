namespace FoodTrust.Core.Restaurants.Models;

public sealed record AdminReviewModerationLogSearchResult(
    IReadOnlyList<AdminReviewModerationLogListItem> Items,
    long TotalCount,
    int Page,
    int PageSize);
