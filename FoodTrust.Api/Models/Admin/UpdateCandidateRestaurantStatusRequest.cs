namespace FoodTrust.Api.Models.Admin;

public sealed record UpdateCandidateRestaurantStatusRequest(
    string Status,
    string? Name,
    string? Address,
    string? PhoneNumber);
