using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Core.Restaurants.Services;

public sealed class RestaurantRankingService(IRestaurantRankingRepository repository) : IRestaurantRankingService
{
    /// <summary>
    /// 依平台排行分數取得餐廳清單。
    /// </summary>
    public Task<IReadOnlyList<RestaurantRankingItem>> GetRestaurantRankingsAsync(int limit)
    {
        return repository.GetRestaurantRankingsAsync(Math.Clamp(limit, 1, 100));
    }
}
