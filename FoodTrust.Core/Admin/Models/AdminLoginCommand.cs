namespace FoodTrust.Core.Admin.Models;

public sealed record AdminLoginCommand(
    string Username,
    string Password);
