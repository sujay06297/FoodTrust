namespace FoodTrust.Api.Models.Admin;

public sealed record ApproveCandidateRestaurantRequest(
    string Name,
    string Address,
    string? PhoneNumber);
