namespace FoodTrust.Core.Admin.Models;

public sealed record AdminBootstrapCommand(
    string Username,
    string Password,
    string DisplayName);
