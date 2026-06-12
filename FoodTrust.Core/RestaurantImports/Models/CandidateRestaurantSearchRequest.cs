namespace FoodTrust.Core.RestaurantImports.Models;

public sealed record CandidateRestaurantSearchRequest(
    string? Status,
    string? Keyword,
    int Page,
    int PageSize);
