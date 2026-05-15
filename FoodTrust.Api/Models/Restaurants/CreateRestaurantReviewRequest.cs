namespace FoodTrust.Api.Models.Restaurants;

public sealed record CreateRestaurantReviewRequest(
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
