namespace FoodTrust.Core.RestaurantImports.Models;

public sealed record RestaurantUpsertResult(
    int InsertedCount,
    int UpdatedCount,
    int LinkedExistingCount)
{
    public int ImportedCount => InsertedCount + UpdatedCount + LinkedExistingCount;
}
