using FoodTrust.Api.Models.Restaurants;
using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoodTrust.Api.Controllers;

[ApiController]
[Route("api/v1/restaurants/{restaurantId:long}/reviews")]
public sealed class RestaurantReviewsController(IRestaurantReviewService reviewService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RestaurantReviewListItem>>> List(
        long restaurantId,
        [FromQuery] int? limit)
    {
        var reviews = await reviewService.GetRestaurantReviewsAsync(restaurantId, limit ?? 20);
        return Ok(reviews);
    }

    [HttpPost]
    public async Task<IActionResult> Create(long restaurantId, [FromBody] CreateRestaurantReviewRequest request)
    {
        var command = new CreateRestaurantReviewCommand(
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
}
