namespace FoodTrust.Core.Restaurants.Models;

public sealed record AdminReviewModerationLogListItem(
    long Id,
    long ReviewId,
    long AdminUserId,
    string AdminUsername,
    string AdminDisplayName,
    string Action,
    string OldStatus,
    string NewStatus,
    string? Reason,
    DateTimeOffset CreatedAt);
