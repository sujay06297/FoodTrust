using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Core.Restaurants.Interfaces;

public interface IRestaurantFavoriteService
{
    /// <summary>
    /// 新增會員餐廳收藏。
    /// </summary>
    Task<bool> AddFavoriteAsync(long userId, long restaurantId);

    /// <summary>
    /// 移除會員餐廳收藏。
    /// </summary>
    Task<bool> RemoveFavoriteAsync(long userId, long restaurantId);

    /// <summary>
    /// 查詢會員收藏餐廳列表。
    /// </summary>
    Task<FavoriteRestaurantSearchResult> SearchFavoritesAsync(long userId, int page, int pageSize);
}
