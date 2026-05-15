namespace FoodTrust.Core.Restaurants.Models;

public sealed record AdminRestaurantReviewListItem(
    long Id,
    long RestaurantId,
    string RestaurantName,
    decimal TasteScore,
    decimal ServiceScore,
    decimal EnvironmentScore,
    decimal ValueScore,
    decimal RevisitScore,
    decimal AverageScore,
    string Content,
    string? ReviewerName,
    DateOnly? VisitDate,
    int? PricePerPerson,
    string? DiningType,
    string? CompanionType,
    string Status,
    bool IsSuspicious,
    bool IsDeleted,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

