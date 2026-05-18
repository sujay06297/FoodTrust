using FoodTrust.Api.Models.Admin;
using FoodTrust.Core.Admin.Interfaces;
using FoodTrust.Core.Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoodTrust.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/auth")]
public sealed class AdminAuthController(IAdminAuthService adminAuthService) : ControllerBase
{
    /// <summary>
    /// 在系統尚未有管理員時建立第一個後台管理員。
    /// </summary>
    [HttpPost("bootstrap")]
    public async Task<ActionResult<AdminBootstrapResult>> Bootstrap([FromBody] BootstrapAdminRequest request)
    {
        var result = await adminAuthService.BootstrapAsync(new AdminBootstrapCommand(
            request.Username,
            request.Password,
            request.DisplayName));

        return result.Created ? Created(string.Empty, result) : Conflict(result);
    }

    /// <summary>
    /// 使用後台管理員帳密登入並取得 JWT。
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AdminLoginResult>> Login([FromBody] LoginAdminRequest request)
    {
        var result = await adminAuthService.LoginAsync(new AdminLoginCommand(
            request.Username,
            request.Password));

        return result is null ? Unauthorized() : Ok(result);
    }

    /// <summary>
    /// 使用 refresh token 輪替後台 access token。
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AdminLoginResult>> Refresh([FromBody] RefreshAdminTokenRequest request)
    {
        var result = await adminAuthService.RefreshAsync(request.RefreshToken);
        return result is null ? Unauthorized() : Ok(result);
    }

    /// <summary>
    /// 撤銷後台 refresh token。
    /// </summary>
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] RevokeAdminRefreshTokenRequest request)
    {
        var revoked = await adminAuthService.RevokeRefreshTokenAsync(request.RefreshToken);
        return revoked ? NoContent() : NotFound();
    }
}
