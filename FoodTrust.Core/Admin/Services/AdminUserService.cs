using FoodTrust.Core.Admin.Interfaces;
using FoodTrust.Core.Admin.Models;

namespace FoodTrust.Core.Admin.Services;

public sealed class AdminUserService(
    IAdminUserRepository repository,
    IPasswordHasher passwordHasher) : IAdminUserService
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

    /// <summary>
    /// 驗證目前密碼並更新登入管理員密碼。
    /// </summary>
    public async Task<bool> ChangePasswordAsync(long currentAdminUserId, string currentPassword, string newPassword)
    {
        if (currentAdminUserId <= 0)
        {
            throw new ArgumentException("Current admin user identifier is required.", nameof(currentAdminUserId));
        }

        if (string.IsNullOrWhiteSpace(currentPassword))
        {
            throw new ArgumentException("Current password is required.", nameof(currentPassword));
        }

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 12)
        {
            throw new ArgumentException("New password must be at least 12 characters.", nameof(newPassword));
        }

        if (currentPassword == newPassword)
        {
            throw new ArgumentException("New password must be different from current password.", nameof(newPassword));
        }

        var adminUser = await repository.FindByIdAsync(currentAdminUserId);
        if (adminUser is null || !adminUser.IsActive)
        {
            return false;
        }

        if (!passwordHasher.Verify(currentPassword, adminUser.PasswordHash))
        {
            throw new ArgumentException("Current password is incorrect.", nameof(currentPassword));
        }

        return await repository.UpdatePasswordHashAsync(
            currentAdminUserId,
            passwordHasher.Hash(newPassword));
    }
}
