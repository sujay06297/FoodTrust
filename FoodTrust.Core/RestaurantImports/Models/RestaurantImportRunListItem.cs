namespace FoodTrust.Core.RestaurantImports.Models;

public sealed record RestaurantImportRunListItem(
    long Id,
    string SourceSystem,
    string SourceUrl,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt,
    string Status,
    int FetchedCount,
    int ImportedCount,
    int SkippedCount,
    string? ErrorMessage);
