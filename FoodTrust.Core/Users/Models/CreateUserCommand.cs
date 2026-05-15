namespace FoodTrust.Core.Users.Models;

public sealed record CreateUserCommand(
    string Email,
    string PasswordHash,
    string DisplayName,
    string Status);
