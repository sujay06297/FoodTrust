using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Core.Restaurants.Interfaces;

public interface IRestaurantReviewRepository
{
    /// <summary>
    /// 為餐廳新增舊版單一分數評分。
    /// </summary>
    Task<bool> AddRestaurantRatingAsync(long id, CreateRestaurantRatingCommand command);

    /// <summary>
    /// 為餐廳新增完整評論。
    /// </summary>
    Task<bool> AddRestaurantReviewAsync(long id, CreateRestaurantReviewCommand command);

    /// <summary>
    /// 取得餐廳已核准的公開評論。
    /// </summary>
    Task<IReadOnlyList<RestaurantReviewListItem>> GetRestaurantReviewsAsync(long id, int limit);

    /// <summary>
    /// 查詢後台評論審核列表。
    /// </summary>
    Task<AdminRestaurantReviewSearchResult> SearchReviewsForAdminAsync(AdminRestaurantReviewSearchRequest request);

    /// <summary>
    /// 更新評論審核狀態。
    /// </summary>
    Task<bool> UpdateReviewStatusAsync(long id, string status, long adminUserId, string? reason);

    /// <summary>
    /// 更新評論可疑標記。
    /// </summary>
    Task<bool> UpdateReviewSuspiciousAsync(long id, bool isSuspicious, long adminUserId, string? reason);

    /// <summary>
    /// 更新評論刪除標記。
    /// </summary>
    Task<bool> UpdateReviewDeletedAsync(long id, bool isDeleted, long adminUserId, string? reason);

    /// <summary>
    /// 查詢指定評論的後台審核紀錄。
    /// </summary>
    Task<IReadOnlyList<AdminReviewModerationLogListItem>> GetReviewModerationLogsAsync(long id, int limit);
}
