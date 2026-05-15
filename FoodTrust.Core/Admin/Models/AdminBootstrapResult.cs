namespace FoodTrust.Core.Admin.Models;

public sealed record AdminBootstrapResult(
    bool Created,
    string? Reason,
    AdminUserSummary? User);
