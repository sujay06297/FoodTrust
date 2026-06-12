namespace FoodTrust.Core.RestaurantImports.Models;

public static class CandidateRestaurantStatus
{
    public const string Pending = "Pending";

    public const string Approved = "Approved";

    public const string Rejected = "Rejected";

    public static bool IsValid(string? status)
    {
        return status is Pending or Approved or Rejected;
    }
}
