using System.Security.Claims;
using FoodTrust.Api.Models.Admin;
using FoodTrust.Core.Admin.Interfaces;
using FoodTrust.Core.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodTrust.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = AdminRole.Admin)]
[Route("api/v1/admin/users")]
public sealed class AdminUsersController(IAdminUserService adminUserService) : ControllerBase
{
    /// <summary>
    /// 查詢後台管理員列表。
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<AdminUserSearchResult>> Search(
        [FromQuery] bool? isActive,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var result = await adminUserService.SearchAsync(page ?? 1, pageSize ?? 20, isActive);
        return Ok(result);
    }

    /// <summary>
    /// 更新後台管理員啟用狀態。
    /// </summary>
    [HttpPatch("{id:long}/active")]
    public async Task<IActionResult> UpdateActive(long id, [FromBody] UpdateAdminUserActiveRequest request)
    {
        var updated = await adminUserService.UpdateActiveAsync(
            id,
            request.IsActive,
            GetCurrentAdminUserId());

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
