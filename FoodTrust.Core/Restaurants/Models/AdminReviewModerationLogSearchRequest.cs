namespace FoodTrust.Core.Restaurants.Models;

public sealed record AdminReviewModerationLogSearchRequest(
    long? ReviewId,
    long? AdminUserId,
    string? Action,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page,
    int PageSize);
