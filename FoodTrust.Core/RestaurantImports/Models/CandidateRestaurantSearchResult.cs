namespace FoodTrust.Core.RestaurantImports.Models;

public sealed record CandidateRestaurantSearchResult(
    IReadOnlyList<CandidateRestaurantListItem> Items,
    int Page,
    int PageSize,
    int TotalCount);
