using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Core.Restaurants.Interfaces;

public interface IRestaurantRankingRepository
{
    Task<IReadOnlyList<RestaurantRankingItem>> GetRestaurantRankingsAsync(int limit);
}
