using FoodTrust.Api.Models.Admin;
using FoodTrust.Core.Admin.Models;
using FoodTrust.Core.RestaurantImports.Interfaces;
using FoodTrust.Core.RestaurantImports.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodTrust.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = AdminRole.Admin)]
[Route("api/v1/admin/candidate-restaurants")]
public sealed class AdminCandidateRestaurantsController(ICandidateRestaurantService candidateRestaurantService)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CandidateRestaurantSearchResult>> Search(
        [FromQuery] string? status,
        [FromQuery] string? keyword,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var request = new CandidateRestaurantSearchRequest(
            status,
            keyword,
            page ?? 1,
            pageSize ?? 20);
        var result = await candidateRestaurantService.SearchAsync(request);

        return Ok(result);
    }

    [HttpPost("{id:long}/approve")]
    public async Task<ActionResult<object>> Approve(long id, [FromBody] ApproveCandidateRestaurantRequest request)
    {
        var restaurantId = await candidateRestaurantService.ApproveAsync(
            new ApproveCandidateRestaurantCommand(
                id,
                request.Name,
                request.Address,
                request.PhoneNumber));

        return restaurantId is null
            ? NotFound()
            : Ok(new { restaurantId });
    }

    [HttpPost("{id:long}/reject")]
    public async Task<IActionResult> Reject(long id)
    {
        var updated = await candidateRestaurantService.RejectAsync(id);
        return updated ? NoContent() : NotFound();
    }
}
