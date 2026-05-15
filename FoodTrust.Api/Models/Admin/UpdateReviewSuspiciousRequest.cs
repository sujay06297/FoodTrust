namespace FoodTrust.Api.Models.Admin;

public sealed record UpdateReviewSuspiciousRequest(
    bool IsSuspicious,
    string? Reason);
