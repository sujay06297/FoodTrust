namespace FoodTrust.Api.Models.Admin;

public sealed record UpdateReviewReportStatusRequest(
    string Status,
    string? ResolutionNote);
