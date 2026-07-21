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

    [HttpPatch("{id:long}/status")]
    public async Task<ActionResult<object>> UpdateStatus(long id, [FromBody] UpdateCandidateRestaurantStatusRequest request)
    {
        if (request.Status == CandidateRestaurantStatus.Rejected)
        {
            var updated = await candidateRestaurantService.RejectAsync(id);
            return updated ? NoContent() : NotFound();
        }

        if (request.Status != CandidateRestaurantStatus.Approved)
        {
            throw new ArgumentException("Invalid candidate restaurant status.", nameof(request.Status));
        }

        var restaurantId = await candidateRestaurantService.ApproveAsync(
            new ApproveCandidateRestaurantCommand(
                id,
                request.Name ?? string.Empty,
                request.Address ?? string.Empty,
                request.PhoneNumber));

        return restaurantId is null
            ? NotFound()
            : Ok(new { restaurantId });
    }
}
