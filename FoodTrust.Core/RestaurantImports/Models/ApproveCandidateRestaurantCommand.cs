namespace FoodTrust.Core.RestaurantImports.Models;

public sealed record ApproveCandidateRestaurantCommand(
    long CandidateId,
    string Name,
    string Address,
    string? PhoneNumber);
