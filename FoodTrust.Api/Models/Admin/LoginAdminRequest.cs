namespace FoodTrust.Api.Models.Admin;

public sealed record LoginAdminRequest(
    string Username,
    string Password);
