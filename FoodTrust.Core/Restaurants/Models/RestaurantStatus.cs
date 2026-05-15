namespace FoodTrust.Core.Restaurants.Models;

public static class RestaurantStatus
{
    public const string Active = "Active";
    public const string Closed = "Closed";
    public const string PendingReview = "PendingReview";

    public static bool IsValid(string? status)
    {
        return status is Active or Closed or PendingReview;
    }
}
