using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Core.Restaurants.Services;

public sealed class RestaurantFavoriteService(IRestaurantFavoriteRepository repository) : IRestaurantFavoriteService
{
    /// <summary>
    /// 驗證並新增會員餐廳收藏。
    /// </summary>
    public Task<bool> AddFavoriteAsync(long userId, long restaurantId)
    {
        ValidateUserAndRestaurant(userId, restaurantId);
        return repository.AddFavoriteAsync(userId, restaurantId);
    }

    /// <summary>
    /// 驗證並移除會員餐廳收藏。
    /// </summary>
    public Task<bool> RemoveFavoriteAsync(long userId, long restaurantId)
    {
        ValidateUserAndRestaurant(userId, restaurantId);
        return repository.RemoveFavoriteAsync(userId, restaurantId);
    }

    /// <summary>
    /// 驗證分頁並查詢會員收藏餐廳列表。
    /// </summary>
    public Task<FavoriteRestaurantSearchResult> SearchFavoritesAsync(long userId, int page, int pageSize)
    {
        if (userId <= 0)
        {
            throw new ArgumentException("User identifier is required.", nameof(userId));
        }

        return repository.SearchFavoritesAsync(
            userId,
            Math.Max(1, page),
            Math.Clamp(pageSize, 1, 200));
    }

    /// <summary>
    /// 驗證會員與餐廳識別碼。
    /// </summary>
    private static void ValidateUserAndRestaurant(long userId, long restaurantId)
    {
        if (userId <= 0)
        {
            throw new ArgumentException("User identifier is required.", nameof(userId));
        }

        if (restaurantId <= 0)
        {
            throw new ArgumentException("Restaurant identifier is required.", nameof(restaurantId));
        }
    }
}
