using FoodTrust.Api.Models.Admin;
using FoodTrust.Core.Admin.Models;
using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodTrust.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = AdminRole.Admin)]
[Route("api/v1/admin/reviews")]
public sealed class AdminRestaurantReviewsController(IRestaurantReviewService reviewService) : ControllerBase
{
    /// <summary>
    /// 查詢後台評論審核列表。
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<AdminRestaurantReviewSearchResult>> Search(
        [FromQuery] string? status,
        [FromQuery] bool? isSuspicious,
        [FromQuery] bool? isDeleted,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var request = new AdminRestaurantReviewSearchRequest(
            status,
            isSuspicious,
            isDeleted,
            page ?? 1,
            pageSize ?? 20);
        var result = await reviewService.SearchReviewsForAdminAsync(request);

        return Ok(result);
    }

    /// <summary>
    /// 更新評論審核狀態。
    /// </summary>
    [HttpPatch("{id:long}/status")]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateReviewStatusRequest request)
    {
        var updated = await reviewService.UpdateReviewStatusAsync(id, request.Status);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>
    /// 更新評論可疑標記。
    /// </summary>
    [HttpPatch("{id:long}/suspicious")]
    public async Task<IActionResult> UpdateSuspicious(long id, [FromBody] UpdateReviewSuspiciousRequest request)
    {
        var updated = await reviewService.UpdateReviewSuspiciousAsync(id, request.IsSuspicious);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>
    /// 更新評論刪除標記。
    /// </summary>
    [HttpPatch("{id:long}/deleted")]
    public async Task<IActionResult> UpdateDeleted(long id, [FromBody] UpdateReviewDeletedRequest request)
    {
        var updated = await reviewService.UpdateReviewDeletedAsync(id, request.IsDeleted);
        return updated ? NoContent() : NotFound();
    }
}
