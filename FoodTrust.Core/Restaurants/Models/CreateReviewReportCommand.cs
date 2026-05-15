namespace FoodTrust.Core.Restaurants.Models;

public sealed record CreateReviewReportCommand(
    string ReasonType,
    string? Content,
    string? ReporterName);
