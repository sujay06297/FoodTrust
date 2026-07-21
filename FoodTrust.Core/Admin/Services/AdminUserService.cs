using FoodTrust.Core.Admin.Domain.ValueObjects;
using FoodTrust.Core.Admin.Interfaces;
using FoodTrust.Core.Admin.Models;
using FoodTrust.Core.Common.Domain;
using FoodTrust.Core.Users.Domain.ValueObjects;

namespace FoodTrust.Core.Admin.Services;

public sealed class AdminUserService(
    IAdminUserRepository repository,
    IPasswordHasher passwordHasher) : IAdminUserService
{
    public Task<AdminUserSearchResult> SearchAsync(int page, int pageSize, bool? isActive)
    {
        var pageRequest = PageRequest.Create(page, pageSize);
        return repository.SearchAsync(pageRequest.Page, pageRequest.PageSize, isActive);
    }

    public Task<bool> UpdateActiveAsync(long id, bool isActive, long currentAdminUserId)
    {
        EntityId.Create(id, nameof(id));
        EntityId.Create(currentAdminUserId, nameof(currentAdminUserId));

        if (!isActive && id == currentAdminUserId)
        {
            throw new ArgumentException("Admin user cannot disable the current signed-in account.", nameof(id));
        }

        return repository.UpdateActiveAsync(id, isActive);
    }

    public Task<bool> UpdateRoleAsync(long id, string role, long currentAdminUserId)
    {
        EntityId.Create(id, nameof(id));
        EntityId.Create(currentAdminUserId, nameof(currentAdminUserId));
        var roleName = AdminRoleName.Create(role);

        if (id == currentAdminUserId && roleName.Value != AdminRole.Admin)
        {
            throw new ArgumentException("Admin user cannot downgrade the current signed-in account.", nameof(id));
        }

        return repository.UpdateRoleAsync(id, roleName.Value);
    }

    public async Task<bool> ChangePasswordAsync(long currentAdminUserId, string currentPassword, string newPassword)
    {
        EntityId.Create(currentAdminUserId, nameof(currentAdminUserId));
        if (string.IsNullOrWhiteSpace(currentPassword))
        {
            throw new ArgumentException("Current password is required.", nameof(currentPassword));
        }

        var nextPassword = AccountPassword.Create(newPassword, nameof(newPassword));
        nextPassword.EnsureDifferentFrom(AccountPassword.Create(currentPassword, nameof(currentPassword)));

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
            passwordHasher.Hash(nextPassword.Value));
    }
}
