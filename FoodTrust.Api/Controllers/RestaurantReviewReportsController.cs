using FoodTrust.Api.Models.Restaurants;
using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoodTrust.Api.Controllers;

[ApiController]
[Route("api/v1/reviews/{reviewId:long}/reports")]
public sealed class RestaurantReviewReportsController(IRestaurantReviewService reviewService) : ControllerBase
{
    /// <summary>
    /// 建立餐廳評論檢舉。
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(long reviewId, [FromBody] CreateReviewReportRequest request)
    {
        var command = new CreateReviewReportCommand(
            request.ReasonType,
            request.Content,
            request.ReporterName);
        var created = await reviewService.CreateReviewReportAsync(reviewId, command);

        return created ? NoContent() : NotFound();
    }
}
