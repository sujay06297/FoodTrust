namespace FoodTrust.Core.Restaurants.Models;

public sealed record AdminRestaurantReviewSearchRequest(
    string? Status,
    bool? IsSuspicious,
    bool? IsDeleted,
    int Page,
    int PageSize);

