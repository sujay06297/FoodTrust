namespace FoodTrust.Core.Restaurants.Models;

public static class ReviewModerationAction
{
    public const string UpdateStatus = "UpdateStatus";

    public const string MarkSuspicious = "MarkSuspicious";

    public const string MarkDeleted = "MarkDeleted";

    /// <summary>
    /// 判斷指定審核操作是否受支援。
    /// </summary>
    public static bool IsValid(string? action)
    {
        return action is UpdateStatus or MarkSuspicious or MarkDeleted;
    }
}
