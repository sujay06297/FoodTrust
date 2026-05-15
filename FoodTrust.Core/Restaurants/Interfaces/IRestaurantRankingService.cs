using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Core.Restaurants.Interfaces;

public interface IRestaurantRankingService
{
    /// <summary>
    /// 依平台排行分數取得餐廳清單。
    /// </summary>
    Task<IReadOnlyList<RestaurantRankingItem>> GetRestaurantRankingsAsync(int limit);
}
