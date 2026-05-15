using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Core.Restaurants.Interfaces;

public interface IRestaurantService
{
    Task<long> CreateRestaurantAsync(CreateRestaurantCommand command);

    Task<bool> UpdateRestaurantAsync(long id, UpdateRestaurantCommand command);

    Task<bool> UpdateRestaurantStatusAsync(long id, string status);

    Task<bool> AddRestaurantRatingAsync(long id, CreateRestaurantRatingCommand command);

    Task<IReadOnlyList<RestaurantRankingItem>> GetRestaurantRankingsAsync(int limit);

    Task<RestaurantSearchResult> SearchRestaurantsAsync(RestaurantSearchRequest request);

    Task<RestaurantDetail?> GetRestaurantAsync(long id);
}
