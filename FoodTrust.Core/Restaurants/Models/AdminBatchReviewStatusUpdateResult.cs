namespace FoodTrust.Core.Restaurants.Models;

public sealed record AdminBatchReviewStatusUpdateResult(
    int RequestedCount,
    int UpdatedCount,
    IReadOnlyList<long> NotFoundReviewIds);
