using FoodTrust.Api.Models.Restaurants;
using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoodTrust.Api.Controllers;

[ApiController]
[Route("api/v1/restaurants")]
public sealed class RestaurantsController(IRestaurantService restaurantService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RestaurantDetail>> Create([FromBody] CreateRestaurantRequest request)
    {
        var command = new CreateRestaurantCommand(request.Name, request.Address, request.PhoneNumber);
        var restaurantId = await restaurantService.CreateRestaurantAsync(command);
        var restaurant = await restaurantService.GetRestaurantAsync(restaurantId);

        return CreatedAtAction(nameof(Get), new { id = restaurantId }, restaurant);
    }

    [HttpGet]
    public async Task<ActionResult<RestaurantSearchResult>> Search(
        [FromQuery] string? keyword,
        [FromQuery] string? status,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var request = new RestaurantSearchRequest(
            keyword,
            status,
            page ?? 1,
            pageSize ?? 20);

        var result = await restaurantService.SearchRestaurantsAsync(request);
        return Ok(result);
    }

    [HttpGet("rankings")]
    public async Task<ActionResult<IReadOnlyList<RestaurantRankingItem>>> Rankings([FromQuery] int? limit)
    {
        var rankings = await restaurantService.GetRestaurantRankingsAsync(limit ?? 20);
        return Ok(rankings);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<RestaurantDetail>> Get(long id)
    {
        var restaurant = await restaurantService.GetRestaurantAsync(id);
        return restaurant is null ? NotFound() : Ok(restaurant);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateRestaurantRequest request)
    {
        var command = new UpdateRestaurantCommand(request.Name, request.Address, request.PhoneNumber);
        var updated = await restaurantService.UpdateRestaurantAsync(id, command);
        return updated ? NoContent() : NotFound();
    }

    [HttpPatch("{id:long}/status")]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateRestaurantStatusRequest request)
    {
        var updated = await restaurantService.UpdateRestaurantStatusAsync(id, request.Status);
        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{id:long}/ratings")]
    public async Task<IActionResult> AddRating(long id, [FromBody] CreateRestaurantRatingRequest request)
    {
        var command = new CreateRestaurantRatingCommand(
            request.Score,
            request.Comment,
            request.ReviewerName);
        var created = await restaurantService.AddRestaurantRatingAsync(id, command);

        return created ? NoContent() : NotFound();
    }
}
