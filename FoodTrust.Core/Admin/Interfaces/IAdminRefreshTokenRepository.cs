using FoodTrust.Core.Admin.Models;

namespace FoodTrust.Core.Admin.Interfaces;

public interface IAdminRefreshTokenRepository
{
    /// <summary>
    /// 建立後台 refresh token 紀錄。
    /// </summary>
    Task<AdminRefreshToken> CreateAsync(CreateAdminRefreshTokenCommand command);

    /// <summary>
    /// 依 token hash 查詢 refresh token。
    /// </summary>
    Task<AdminRefreshToken?> FindByTokenHashAsync(string tokenHash);

    /// <summary>
    /// 撤銷 refresh token。
    /// </summary>
    Task<bool> RevokeAsync(long id, DateTime revokedAtUtc);
}
