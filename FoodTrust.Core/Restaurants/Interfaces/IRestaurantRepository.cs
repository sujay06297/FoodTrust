using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Core.Restaurants.Interfaces;

public interface IRestaurantRepository
{
    /// <summary>
    /// 建立餐廳並回傳產生的識別碼。
    /// </summary>
    Task<long> CreateRestaurantAsync(CreateRestaurantCommand command);

    /// <summary>
    /// 更新餐廳可編輯的基本資料欄位。
    /// </summary>
    Task<bool> UpdateRestaurantAsync(long id, UpdateRestaurantCommand command);

    /// <summary>
    /// 更新餐廳目前的生命週期狀態。
    /// </summary>
    Task<bool> UpdateRestaurantStatusAsync(long id, string status);

    /// <summary>
    /// 使用指定篩選條件與分頁設定查詢餐廳。
    /// </summary>
    Task<RestaurantSearchResult> SearchRestaurantsAsync(RestaurantSearchRequest request);

    /// <summary>
    /// 依識別碼取得餐廳詳細資料。
    /// </summary>
    Task<RestaurantDetail?> GetRestaurantAsync(long id);
}
