namespace FoodTrust.Api.Models.Restaurants;

public sealed record CreateReviewReportRequest(
    string ReasonType,
    string? Content,
    string? ReporterName);
