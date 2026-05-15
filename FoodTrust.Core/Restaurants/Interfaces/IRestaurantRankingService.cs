using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Core.Restaurants.Interfaces;

public interface IRestaurantRankingService
{
    Task<IReadOnlyList<RestaurantRankingItem>> GetRestaurantRankingsAsync(int limit);
}
