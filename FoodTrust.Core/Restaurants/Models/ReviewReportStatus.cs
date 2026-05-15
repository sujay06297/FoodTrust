namespace FoodTrust.Core.Restaurants.Models;

public static class ReviewReportStatus
{
    public const string Pending = "Pending";

    public const string Resolved = "Resolved";

    public const string Rejected = "Rejected";

    /// <summary>
    /// 判斷指定的檢舉狀態是否受支援。
    /// </summary>
    public static bool IsValid(string? status)
    {
        return status is Pending or Resolved or Rejected;
    }
}
