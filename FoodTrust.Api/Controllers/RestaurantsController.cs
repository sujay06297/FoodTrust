using FoodTrust.Api.Models.Restaurants;
using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoodTrust.Api.Controllers;

[ApiController]
[Route("api/v1/restaurants")]
public sealed class RestaurantsController(IRestaurantService restaurantService) : ControllerBase
{
    /// <summary>
    /// 建立餐廳資料。
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RestaurantDetail>> Create([FromBody] CreateRestaurantRequest request)
    {
        var command = new CreateRestaurantCommand(
            request.Name,
            request.Address,
            request.PhoneNumber,
            request.BranchName,
            request.City,
            request.District,
            request.Latitude,
            request.Longitude,
            request.OpeningHours,
            request.PriceMin,
            request.PriceMax,
            request.CuisineType,
            request.Tags,
            request.Description,
            request.OfficialUrl,
            request.GoogleMapUrl);
        var restaurantId = await restaurantService.CreateRestaurantAsync(command);
        var restaurant = await restaurantService.GetRestaurantAsync(restaurantId);

        return CreatedAtAction(nameof(Get), new { id = restaurantId }, restaurant);
    }

    /// <summary>
    /// 使用篩選條件、分頁與支援的排序選項查詢餐廳。
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<RestaurantSearchResult>> Search(
        [FromQuery] string? keyword,
        [FromQuery] string? status,
        [FromQuery] string? city,
        [FromQuery] string? district,
        [FromQuery] string? cuisineType,
        [FromQuery] int? priceMin,
        [FromQuery] int? priceMax,
        [FromQuery] decimal? minScore,
        [FromQuery] string? sortBy,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var request = new RestaurantSearchRequest(
            keyword,
            status,
            city,
            district,
            cuisineType,
            priceMin,
            priceMax,
            minScore,
            sortBy,
            page ?? 1,
            pageSize ?? 20);

        var result = await restaurantService.SearchRestaurantsAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// 依識別碼取得餐廳。
    /// </summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<RestaurantDetail>> Get(long id)
    {
        var restaurant = await restaurantService.GetRestaurantAsync(id);
        return restaurant is null ? NotFound() : Ok(restaurant);
    }

    /// <summary>
    /// 更新餐廳資料。
    /// </summary>
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateRestaurantRequest request)
    {
        var command = new UpdateRestaurantCommand(
            request.Name,
            request.Address,
            request.PhoneNumber,
            request.BranchName,
            request.City,
            request.District,
            request.Latitude,
            request.Longitude,
            request.OpeningHours,
            request.PriceMin,
            request.PriceMax,
            request.CuisineType,
            request.Tags,
            request.Description,
            request.OfficialUrl,
            request.GoogleMapUrl);
        var updated = await restaurantService.UpdateRestaurantAsync(id, command);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>
    /// 更新餐廳狀態。
    /// </summary>
    [HttpPatch("{id:long}/status")]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateRestaurantStatusRequest request)
    {
        var updated = await restaurantService.UpdateRestaurantStatusAsync(id, request.Status);
        return updated ? NoContent() : NotFound();
    }
}
