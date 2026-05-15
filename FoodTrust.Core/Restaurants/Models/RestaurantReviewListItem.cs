namespace FoodTrust.Core.Restaurants.Models;

public sealed record RestaurantReviewListItem(
    long Id,
    long RestaurantId,
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
    DateTimeOffset CreatedAt);
