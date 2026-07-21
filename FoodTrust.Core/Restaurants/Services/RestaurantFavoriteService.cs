using FoodTrust.Core.Common.Domain;
using FoodTrust.Core.Restaurants.Domain;
using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Core.Restaurants.Services;

public sealed class RestaurantFavoriteService(IRestaurantFavoriteRepository repository) : IRestaurantFavoriteService
{
    public Task<bool> AddFavoriteAsync(long userId, long restaurantId)
    {
        var favorite = FavoriteRestaurant.Create(userId, restaurantId);
        return repository.AddFavoriteAsync(favorite.UserId.Value, favorite.RestaurantId.Value);
    }

    public Task<bool> RemoveFavoriteAsync(long userId, long restaurantId)
    {
        var favorite = FavoriteRestaurant.Create(userId, restaurantId);
        return repository.RemoveFavoriteAsync(favorite.UserId.Value, favorite.RestaurantId.Value);
    }

    public Task<FavoriteRestaurantSearchResult> SearchFavoritesAsync(long userId, int page, int pageSize)
    {
        var ownerId = EntityId.Create(userId, nameof(userId));
        var pageRequest = PageRequest.Create(page, pageSize);
        return repository.SearchFavoritesAsync(ownerId.Value, pageRequest.Page, pageRequest.PageSize);
    }
}
