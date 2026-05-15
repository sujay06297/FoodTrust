namespace FoodTrust.Core.RestaurantImports.Models;

public sealed record RestaurantImportRecord(
    string SourceSystem,
    string SourceKey,
    string Name,
    string Address,
    string? PhoneNumber,
    string RawPayload);
