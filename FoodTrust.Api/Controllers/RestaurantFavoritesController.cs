using System.Security.Claims;
using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Models;
using FoodTrust.Core.Users.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodTrust.Api.Controllers;

[ApiController]
[Authorize(Roles = UserRole.User)]
public sealed class RestaurantFavoritesController(IRestaurantFavoriteService favoriteService) : ControllerBase
{
    /// <summary>
    /// 收藏指定餐廳。
    /// </summary>
    [HttpPost("api/v1/restaurants/{restaurantId:long}/favorite")]
    public async Task<IActionResult> Add(long restaurantId)
    {
        var added = await favoriteService.AddFavoriteAsync(GetCurrentUserId(), restaurantId);
        return added ? NoContent() : NotFound();
    }

    /// <summary>
    /// 取消收藏指定餐廳。
    /// </summary>
    [HttpDelete("api/v1/restaurants/{restaurantId:long}/favorite")]
    public async Task<IActionResult> Remove(long restaurantId)
    {
        var removed = await favoriteService.RemoveFavoriteAsync(GetCurrentUserId(), restaurantId);
        return removed ? NoContent() : NotFound();
    }

    /// <summary>
    /// 查詢目前會員收藏餐廳。
    /// </summary>
    [HttpGet("api/v1/users/me/favorite-restaurants")]
    public async Task<ActionResult<FavoriteRestaurantSearchResult>> Search(
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var result = await favoriteService.SearchFavoritesAsync(
            GetCurrentUserId(),
            page ?? 1,
            pageSize ?? 20);

        return Ok(result);
    }

    /// <summary>
    /// 取得目前登入會員的使用者識別碼。
    /// </summary>
    private long GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(value, out var userId))
        {
            throw new InvalidOperationException("Invalid user identifier.");
        }

        return userId;
    }
}
