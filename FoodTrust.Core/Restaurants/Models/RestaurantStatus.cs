namespace FoodTrust.Core.Restaurants.Models;

public static class RestaurantStatus
{
    public const string Active = "Active";
    public const string Closed = "Closed";
    public const string PendingReview = "PendingReview";

    /// <summary>
    /// 判斷指定的餐廳狀態是否受支援。
    /// </summary>
    public static bool IsValid(string? status)
    {
        return status is Active or Closed or PendingReview;
    }
}
