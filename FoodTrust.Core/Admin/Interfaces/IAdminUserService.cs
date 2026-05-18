using FoodTrust.Core.Admin.Models;

namespace FoodTrust.Core.Admin.Interfaces;

public interface IAdminUserService
{
    /// <summary>
    /// 查詢後台管理員列表。
    /// </summary>
    Task<AdminUserSearchResult> SearchAsync(int page, int pageSize, bool? isActive);

    /// <summary>
    /// 更新後台管理員啟用狀態。
    /// </summary>
    Task<bool> UpdateActiveAsync(long id, bool isActive, long currentAdminUserId);
}
