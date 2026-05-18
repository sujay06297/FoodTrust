namespace FoodTrust.Core.Admin.Models;

public sealed record AdminUserSearchResult(
    IReadOnlyList<AdminUserSummary> Items,
    int Page,
    int PageSize,
    long TotalCount);
