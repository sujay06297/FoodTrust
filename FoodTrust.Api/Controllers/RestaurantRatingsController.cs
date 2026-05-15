using FoodTrust.Api.Models.Restaurants;
using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoodTrust.Api.Controllers;

[ApiController]
[Route("api/v1/restaurants/{restaurantId:long}/ratings")]
public sealed class RestaurantRatingsController(IRestaurantReviewService reviewService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(long restaurantId, [FromBody] CreateRestaurantRatingRequest request)
    {
        var command = new CreateRestaurantRatingCommand(
            request.Score,
            request.Comment,
            request.ReviewerName);
        var created = await reviewService.AddRestaurantRatingAsync(restaurantId, command);

        return created ? NoContent() : NotFound();
    }
}
