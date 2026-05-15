using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Core.Restaurants.Interfaces;

public interface IRestaurantRepository
{
    Task<long> CreateRestaurantAsync(CreateRestaurantCommand command);

    Task<bool> UpdateRestaurantAsync(long id, UpdateRestaurantCommand command);

    Task<bool> UpdateRestaurantStatusAsync(long id, string status);

    Task<RestaurantSearchResult> SearchRestaurantsAsync(RestaurantSearchRequest request);

    Task<RestaurantDetail?> GetRestaurantAsync(long id);
}
