namespace FoodTrust.Core.Users.Models;

public sealed record LoginUserCommand(
    string Email,
    string Password);
