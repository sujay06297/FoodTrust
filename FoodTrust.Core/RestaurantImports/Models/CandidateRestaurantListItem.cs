namespace FoodTrust.Core.RestaurantImports.Models;

public sealed record CandidateRestaurantListItem(
    long Id,
    string SourceSystem,
    string SourceKey,
    string RawName,
    string RawAddress,
    string? RawPhoneNumber,
    string? SuggestedName,
    string Status,
    long? LinkedRestaurantId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
