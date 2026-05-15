using FoodTrust.Core.RestaurantImports.Interfaces;
using FoodTrust.Core.RestaurantImports.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoodTrust.Api.Controllers;

[ApiController]
[Route("api/v1/restaurant-import-runs")]
public sealed class RestaurantImportRunsController(IRestaurantImportRunService importRunService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RestaurantImportRunListItem>>> List([FromQuery] int? limit)
    {
        var runs = await importRunService.GetImportRunsAsync(limit ?? 20);
        return Ok(runs);
    }
}
