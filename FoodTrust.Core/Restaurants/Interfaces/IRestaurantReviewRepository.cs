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
}
