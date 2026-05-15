namespace FoodTrust.Core.Restaurants.Models;

public sealed record AdminReviewReportListItem(
    long Id,
    long ReviewId,
    long RestaurantId,
    string RestaurantName,
    string ReasonType,
    string? Content,
    string? ReporterName,
    string Status,
    string ReviewStatus,
    string ReviewContent,
    string? ResolutionNote,
    long? ResolvedByAdminUserId,
    string? ResolvedByAdminUsername,
    DateTimeOffset? ResolvedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
