using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Core.Restaurants.Interfaces;

public interface IRestaurantReviewRepository
{
    Task<bool> AddRestaurantRatingAsync(long id, CreateRestaurantRatingCommand command);

    Task<bool> AddRestaurantReviewAsync(long id, CreateRestaurantReviewCommand command);

    Task<IReadOnlyList<RestaurantReviewListItem>> GetRestaurantReviewsAsync(long id, int limit);
}
