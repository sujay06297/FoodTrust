namespace FoodTrust.Api.Models.Users;

public sealed record LoginUserRequest(
    string Email,
    string Password);
