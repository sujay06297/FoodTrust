using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Core.Restaurants.Interfaces;

public interface IRestaurantService
{
    /// <summary>
    /// 驗證並建立餐廳。
    /// </summary>
    Task<long> CreateRestaurantAsync(CreateRestaurantCommand command);

    /// <summary>
    /// 驗證並更新餐廳。
    /// </summary>
    Task<bool> UpdateRestaurantAsync(long id, UpdateRestaurantCommand command);

    /// <summary>
    /// 驗證並更新餐廳狀態。
    /// </summary>
    Task<bool> UpdateRestaurantStatusAsync(long id, string status);

    /// <summary>
    /// 驗證並查詢餐廳。
    /// </summary>
    Task<RestaurantSearchResult> SearchRestaurantsAsync(RestaurantSearchRequest request);

    /// <summary>
    /// 依識別碼取得餐廳詳細資料。
    /// </summary>
    Task<RestaurantDetail?> GetRestaurantAsync(long id);
}
