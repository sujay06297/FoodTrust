namespace FoodTrust.Core.Restaurants.Models;

public static class RestaurantReviewStatus
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Hidden = "Hidden";
    public const string Suspicious = "Suspicious";
    public const string Deleted = "Deleted";

    /// <summary>
    /// 判斷指定的評論狀態是否受支援。
    /// </summary>
    public static bool IsValid(string? status)
    {
        return status is Pending or Approved or Rejected or Hidden or Suspicious or Deleted;
    }
}
