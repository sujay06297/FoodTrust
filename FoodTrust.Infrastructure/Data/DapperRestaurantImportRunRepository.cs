using Dapper;
using FoodTrust.Core.RestaurantImports.Interfaces;
using FoodTrust.Core.RestaurantImports.Models;

namespace FoodTrust.Infrastructure.Data;

public sealed class DapperRestaurantImportRunRepository(MySqlConnectionFactory connectionFactory) : IRestaurantImportRunRepository
{
    public async Task<long> StartImportRunAsync(string sourceSystem, string sourceUrl, DateTimeOffset startedAt)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        await connection.ExecuteAsync("""
            INSERT INTO restaurant_import_runs (source_system, source_url, started_at, status)
            VALUES (@SourceSystem, @SourceUrl, @StartedAt, 'Running');
            """, new
        {
            SourceSystem = sourceSystem,
            SourceUrl = sourceUrl,
            StartedAt = startedAt.UtcDateTime
        });

        return await connection.ExecuteScalarAsync<long>("SELECT LAST_INSERT_ID();");
    }

    public async Task CompleteImportRunAsync(
        long runId,
        int fetchedCount,
        int importedCount,
        int skippedCount,
        DateTimeOffset finishedAt)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        await connection.ExecuteAsync("""
            UPDATE restaurant_import_runs
            SET finished_at = @FinishedAt,
                status = 'Succeeded',
                fetched_count = @FetchedCount,
                imported_count = @ImportedCount,
                skipped_count = @SkippedCount
            WHERE id = @RunId;
            """, new
        {
            RunId = runId,
            FinishedAt = finishedAt.UtcDateTime,
            FetchedCount = fetchedCount,
            ImportedCount = importedCount,
            SkippedCount = skippedCount
        });
    }

    public async Task FailImportRunAsync(long runId, string errorMessage, DateTimeOffset finishedAt)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        await connection.ExecuteAsync("""
            UPDATE restaurant_import_runs
            SET finished_at = @FinishedAt,
                status = 'Failed',
                error_message = @ErrorMessage
            WHERE id = @RunId;
            """, new
        {
            RunId = runId,
            FinishedAt = finishedAt.UtcDateTime,
            ErrorMessage = errorMessage
        });
    }

    public async Task<IReadOnlyList<RestaurantImportRunListItem>> GetImportRunsAsync(int limit)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var rows = await connection.QueryAsync<RestaurantImportRunRow>("""
            SELECT
                id,
                source_system AS SourceSystem,
                source_url AS SourceUrl,
                started_at AS StartedAt,
                finished_at AS FinishedAt,
                status,
                fetched_count AS FetchedCount,
                imported_count AS ImportedCount,
                skipped_count AS SkippedCount,
                error_message AS ErrorMessage
            FROM restaurant_import_runs
            ORDER BY id DESC
            LIMIT @Limit;
            """, new { Limit = Math.Clamp(limit, 1, 200) });

        return rows.Select(ToImportRunListItem).ToArray();
    }

    private static RestaurantImportRunListItem ToImportRunListItem(RestaurantImportRunRow row)
    {
        return new RestaurantImportRunListItem(
            row.Id,
            row.SourceSystem,
            row.SourceUrl,
            ToUtcOffset(row.StartedAt),
            row.FinishedAt is null ? null : ToUtcOffset(row.FinishedAt.Value),
            row.Status,
            row.FetchedCount,
            row.ImportedCount,
            row.SkippedCount,
            row.ErrorMessage);
    }

    private static DateTimeOffset ToUtcOffset(DateTime value)
    {
        return new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }

    private sealed record RestaurantImportRunRow(
        long Id,
        string SourceSystem,
        string SourceUrl,
        DateTime StartedAt,
        DateTime? FinishedAt,
        string Status,
        int FetchedCount,
        int ImportedCount,
        int SkippedCount,
        string? ErrorMessage);
}
