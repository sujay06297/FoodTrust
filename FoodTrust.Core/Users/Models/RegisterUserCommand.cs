namespace FoodTrust.Core.Users.Models;

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string DisplayName);
