namespace FoodTrust.Core.Restaurants.Models;

public sealed record AdminReviewReportSearchResult(
    IReadOnlyList<AdminReviewReportListItem> Items,
    int TotalCount,
    int Page,
    int PageSize);
