using FoodTrust.Core.RestaurantImports.Interfaces;
using FoodTrust.Core.RestaurantImports.Domain;

namespace FoodTrust.Core.RestaurantImports.Services;

public sealed class RestaurantImportService(
    IRestaurantImportSource source,
    IRestaurantImportRunRepository importRunRepository,
    IRestaurantImportTargetRepository importTargetRepository) : IRestaurantImportService
{
    /// <summary>
    /// 從設定的來源匯入餐廳，並記錄匯入執行結果。
    /// </summary>
    public async Task ImportAsync(int batchSize, CancellationToken cancellationToken)
    {
        var importBatchSize = ImportBatchSize.Create(batchSize);
        var runId = await importRunRepository.StartImportRunAsync(
            source.SourceSystem,
            source.SourceUrl,
            DateTimeOffset.UtcNow);

        var fetchedCount = 0;
        var importedCount = 0;
        var skippedCount = 0;

        try
        {
            var records = await source.FetchRestaurantsAsync(cancellationToken);
            fetchedCount = records.Count;
            var distinctRecords = records
                .GroupBy(record => new { record.SourceSystem, record.SourceKey })
                .Select(group => group.First())
                .ToArray();

            foreach (var batch in distinctRecords.Chunk(importBatchSize.Value))
            {
                var result = await importTargetRepository.UpsertRestaurantsAsync(batch);
                importedCount += result.ImportedCount;
            }

            skippedCount = fetchedCount - importedCount;

            await importRunRepository.CompleteImportRunAsync(
                runId,
                fetchedCount,
                importedCount,
                skippedCount,
                DateTimeOffset.UtcNow);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            await importRunRepository.FailImportRunAsync(runId, ex.Message, DateTimeOffset.UtcNow);
            throw;
        }
    }
}
