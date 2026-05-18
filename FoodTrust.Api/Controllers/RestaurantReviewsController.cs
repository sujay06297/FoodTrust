using System.Security.Claims;
using FoodTrust.Api.Models.Restaurants;
using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Models;
using FoodTrust.Core.Users.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodTrust.Api.Controllers;

[ApiController]
[Route("api/v1/restaurants/{restaurantId:long}/reviews")]
public sealed class RestaurantReviewsController(IRestaurantReviewService reviewService) : ControllerBase
{
    /// <summary>
    /// 列出餐廳已核准的公開評論。
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RestaurantReviewListItem>>> List(
        long restaurantId,
        [FromQuery] int? limit)
    {
        var reviews = await reviewService.GetRestaurantReviewsAsync(restaurantId, limit ?? 20);
        return Ok(reviews);
    }

    /// <summary>
    /// 建立包含分類分數的完整餐廳評論。
    /// </summary>
    [Authorize(Roles = UserRole.User)]
    [HttpPost]
    public async Task<IActionResult> Create(long restaurantId, [FromBody] CreateRestaurantReviewRequest request)
    {
        var command = new CreateRestaurantReviewCommand(
            GetCurrentUserId(),
            request.TasteScore,
            request.ServiceScore,
            request.EnvironmentScore,
            request.ValueScore,
            request.RevisitScore,
            request.Content,
            request.ReviewerName,
            request.VisitDate,
            request.PricePerPerson,
            request.DiningType,
            request.CompanionType);
        var created = await reviewService.AddRestaurantReviewAsync(restaurantId, command);

        return created ? NoContent() : NotFound();
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
