using FoodTrust.Core.Admin.Interfaces;
using FoodTrust.Core.Admin.Models;

namespace FoodTrust.Core.Admin.Services;

public sealed class AdminUserService(IAdminUserRepository repository) : IAdminUserService
{
    /// <summary>
    /// 驗證分頁並查詢後台管理員列表。
    /// </summary>
    public Task<AdminUserSearchResult> SearchAsync(int page, int pageSize, bool? isActive)
    {
        return repository.SearchAsync(
            Math.Max(1, page),
            Math.Clamp(pageSize, 1, 200),
            isActive);
    }

    /// <summary>
    /// 驗證並更新後台管理員啟用狀態。
    /// </summary>
    public Task<bool> UpdateActiveAsync(long id, bool isActive, long currentAdminUserId)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Admin user identifier is required.", nameof(id));
        }

        if (currentAdminUserId <= 0)
        {
            throw new ArgumentException("Current admin user identifier is required.", nameof(currentAdminUserId));
        }

        if (!isActive && id == currentAdminUserId)
        {
            throw new ArgumentException("Admin user cannot disable the current signed-in account.", nameof(id));
        }

        return repository.UpdateActiveAsync(id, isActive);
    }
}
