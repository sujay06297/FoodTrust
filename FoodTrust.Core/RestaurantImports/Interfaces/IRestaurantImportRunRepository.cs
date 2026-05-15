using FoodTrust.Core.RestaurantImports.Models;

namespace FoodTrust.Core.RestaurantImports.Interfaces;

public interface IRestaurantImportRunRepository
{
    Task<long> StartImportRunAsync(string sourceSystem, string sourceUrl, DateTimeOffset startedAt);

    Task CompleteImportRunAsync(
        long runId,
        int fetchedCount,
        int importedCount,
        int skippedCount,
        DateTimeOffset finishedAt);

    Task FailImportRunAsync(long runId, string errorMessage, DateTimeOffset finishedAt);

    Task<IReadOnlyList<RestaurantImportRunListItem>> GetImportRunsAsync(int limit);
}
