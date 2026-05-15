namespace FoodTrust.Api.Models.Admin;

public sealed record BootstrapAdminRequest(
    string Username,
    string Password,
    string DisplayName);
