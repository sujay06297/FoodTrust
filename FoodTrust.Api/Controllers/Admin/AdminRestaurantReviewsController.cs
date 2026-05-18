using System.Security.Claims;
using FoodTrust.Api.Models.Admin;
using FoodTrust.Api.Security;
using FoodTrust.Core.Admin.Models;
using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodTrust.Api.Controllers.Admin;

[ApiController]
[Authorize(Policy = AdminPolicies.ReviewModeration)]
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
        var updated = await reviewService.UpdateReviewStatusAsync(
            id,
            request.Status,
            GetCurrentAdminUserId(),
            request.Reason);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>
    /// 批次更新評論審核狀態。
    /// </summary>
    [HttpPatch("status")]
    public async Task<ActionResult<AdminBatchReviewStatusUpdateResult>> BatchUpdateStatus(
        [FromBody] BatchUpdateReviewStatusRequest request)
    {
        var result = await reviewService.BatchUpdateReviewStatusAsync(
            request.ReviewIds,
            request.Status,
            GetCurrentAdminUserId(),
            request.Reason);

        return Ok(result);
    }

    /// <summary>
    /// 更新評論可疑標記。
    /// </summary>
    [HttpPatch("{id:long}/suspicious")]
    public async Task<IActionResult> UpdateSuspicious(long id, [FromBody] UpdateReviewSuspiciousRequest request)
    {
        var updated = await reviewService.UpdateReviewSuspiciousAsync(
            id,
            request.IsSuspicious,
            GetCurrentAdminUserId(),
            request.Reason);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>
    /// 更新評論刪除標記。
    /// </summary>
    [HttpPatch("{id:long}/deleted")]
    public async Task<IActionResult> UpdateDeleted(long id, [FromBody] UpdateReviewDeletedRequest request)
    {
        var updated = await reviewService.UpdateReviewDeletedAsync(
            id,
            request.IsDeleted,
            GetCurrentAdminUserId(),
            request.Reason);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>
    /// 查詢指定評論的後台審核紀錄。
    /// </summary>
    [HttpGet("{id:long}/moderation-logs")]
    public async Task<ActionResult<IReadOnlyList<AdminReviewModerationLogListItem>>> GetModerationLogs(
        long id,
        [FromQuery] int? limit)
    {
        var logs = await reviewService.GetReviewModerationLogsAsync(id, limit ?? 50);
        return Ok(logs);
    }

    /// <summary>
    /// 搜尋後台評論審核紀錄。
    /// </summary>
    [HttpGet("moderation-logs")]
    public async Task<ActionResult<AdminReviewModerationLogSearchResult>> SearchModerationLogs(
        [FromQuery] long? reviewId,
        [FromQuery] long? adminUserId,
        [FromQuery] string? action,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var request = new AdminReviewModerationLogSearchRequest(
            reviewId,
            adminUserId,
            action,
            from,
            to,
            page ?? 1,
            pageSize ?? 20);
        var result = await reviewService.SearchReviewModerationLogsAsync(request);

        return Ok(result);
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
