namespace FoodTrust.Core.Restaurants.Models;

public sealed record CreateRestaurantReviewCommand(
    long UserId,
    decimal TasteScore,
    decimal ServiceScore,
    decimal EnvironmentScore,
    decimal ValueScore,
    decimal RevisitScore,
    string Content,
    string? ReviewerName,
    DateOnly? VisitDate,
    int? PricePerPerson,
    string? DiningType,
    string? CompanionType);
