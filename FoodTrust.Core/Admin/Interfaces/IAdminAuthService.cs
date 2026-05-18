using FoodTrust.Core.Admin.Models;

namespace FoodTrust.Core.Admin.Interfaces;

public interface IAdminAuthService
{
    /// <summary>
    /// 使用帳號密碼登入後台並簽發存取權杖。
    /// </summary>
    Task<AdminLoginResult?> LoginAsync(AdminLoginCommand command);

    /// <summary>
    /// 使用 refresh token 輪替後台存取權杖。
    /// </summary>
    Task<AdminLoginResult?> RefreshAsync(string refreshToken);

    /// <summary>
    /// 撤銷後台 refresh token。
    /// </summary>
    Task<bool> RevokeRefreshTokenAsync(string refreshToken);

    /// <summary>
    /// 在尚未有管理員時建立第一個後台管理員。
    /// </summary>
    Task<AdminBootstrapResult> BootstrapAsync(AdminBootstrapCommand command);
}
