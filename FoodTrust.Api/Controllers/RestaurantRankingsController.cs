using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoodTrust.Api.Controllers;

[ApiController]
[Route("api/v1/restaurants/rankings")]
public sealed class RestaurantRankingsController(IRestaurantRankingService rankingService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RestaurantRankingItem>>> List([FromQuery] int? limit)
    {
        var rankings = await rankingService.GetRestaurantRankingsAsync(limit ?? 20);
        return Ok(rankings);
    }
}
