using FoodTrust.Api.Models.Users;
using FoodTrust.Core.Users.Interfaces;
using FoodTrust.Core.Users.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoodTrust.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(IUserAuthService userAuthService) : ControllerBase
{
    /// <summary>
    /// 註冊一般會員並取得 JWT。
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<UserAuthResult>> Register([FromBody] RegisterUserRequest request)
    {
        var result = await userAuthService.RegisterAsync(new RegisterUserCommand(
            request.Email,
            request.Password,
            request.DisplayName));

        return Created(string.Empty, result);
    }

    /// <summary>
    /// 使用一般會員帳密登入並取得 JWT。
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<UserAuthResult>> Login([FromBody] LoginUserRequest request)
    {
        var result = await userAuthService.LoginAsync(new LoginUserCommand(
            request.Email,
            request.Password));

        return result is null ? Unauthorized() : Ok(result);
    }
}
