namespace FoodTrust.Core.Admin.Models;

public sealed record CreateAdminUserCommand(
    string Username,
    string PasswordHash,
    string DisplayName,
    string Role);
