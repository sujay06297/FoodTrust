namespace FoodTrust.Core.Restaurants.Models;

public sealed record AdminReviewReportSearchRequest(
    string? Status,
    int Page,
    int PageSize);
