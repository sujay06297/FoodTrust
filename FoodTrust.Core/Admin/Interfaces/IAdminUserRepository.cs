using FoodTrust.Core.Admin.Models;

namespace FoodTrust.Core.Admin.Interfaces;

public interface IAdminUserRepository
{
    /// <summary>
    /// 判斷系統是否已存在任何管理員。
    /// </summary>
    Task<bool> HasAnyAsync();

    /// <summary>
    /// 依帳號查詢管理員。
    /// </summary>
    Task<AdminUser?> FindByUsernameAsync(string username);

    /// <summary>
    /// 建立新的後台管理員。
    /// </summary>
    Task<AdminUser> CreateAsync(CreateAdminUserCommand command);

    /// <summary>
    /// 查詢後台管理員列表。
    /// </summary>
    Task<AdminUserSearchResult> SearchAsync(int page, int pageSize, bool? isActive);

    /// <summary>
    /// 更新後台管理員啟用狀態。
    /// </summary>
    Task<bool> UpdateActiveAsync(long id, bool isActive);
}
