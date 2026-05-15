using System.Security.Claims;
using FoodTrust.Api.Models.Admin;
using FoodTrust.Core.Admin.Models;
using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodTrust.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = AdminRole.Admin)]
[Route("api/v1/admin/review-reports")]
public sealed class AdminReviewReportsController(IRestaurantReviewService reviewService) : ControllerBase
{
    /// <summary>
    /// 查詢後台評論檢舉列表。
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<AdminReviewReportSearchResult>> Search(
        [FromQuery] string? status,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var request = new AdminReviewReportSearchRequest(
            status,
            page ?? 1,
            pageSize ?? 20);
        var result = await reviewService.SearchReviewReportsForAdminAsync(request);

        return Ok(result);
    }

    /// <summary>
    /// 更新評論檢舉處理狀態。
    /// </summary>
    [HttpPatch("{reportId:long}/status")]
    public async Task<IActionResult> UpdateStatus(long reportId, [FromBody] UpdateReviewReportStatusRequest request)
    {
        var updated = await reviewService.UpdateReviewReportStatusAsync(
            reportId,
            request.Status,
            GetCurrentAdminUserId(),
            request.ResolutionNote);

        return updated ? NoContent() : NotFound();
    }

    /// <summary>
    /// 取得目前登入管理員的使用者識別碼。
    /// </summary>
    private long GetCurrentAdminUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(value, out var adminUserId))
        {
            throw new InvalidOperationException("Invalid admin user identifier.");
        }

        return adminUserId;
    }
}
