namespace FoodTrust.Api.Models.Admin;

public sealed record ChangeAdminPasswordRequest(
    string CurrentPassword,
    string NewPassword);
