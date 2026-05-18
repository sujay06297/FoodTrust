namespace FoodTrust.Api.Models.Admin;

public sealed record BatchUpdateReviewStatusRequest(
    IReadOnlyList<long> ReviewIds,
    string Status,
    string? Reason);
