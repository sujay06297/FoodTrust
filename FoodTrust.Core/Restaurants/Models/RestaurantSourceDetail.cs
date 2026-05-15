namespace FoodTrust.Core.Restaurants.Models;

public sealed record RestaurantSourceDetail(
    string SourceSystem,
    string SourceKey,
    string RawName,
    string RawAddress,
    string? RawPhoneNumber,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
