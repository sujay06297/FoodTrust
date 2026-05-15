using FoodTrust.Core.Admin.Interfaces;
using FoodTrust.Core.Admin.Models;

namespace FoodTrust.Core.Admin.Services;

public sealed class AdminAuthService(
    IAdminUserRepository adminUserRepository,
    IPasswordHasher passwordHasher,
    IAdminTokenGenerator tokenGenerator) : IAdminAuthService
{
    /// <summary>
    /// 使用帳號密碼登入後台並簽發存取權杖。
    /// </summary>
    public async Task<AdminLoginResult?> LoginAsync(AdminLoginCommand command)
    {
        var username = NormalizeUsername(command.Username);
        if (username is null || string.IsNullOrWhiteSpace(command.Password))
        {
            return null;
        }

        var adminUser = await adminUserRepository.FindByUsernameAsync(username);
        if (adminUser is null ||
            !adminUser.IsActive ||
            !passwordHasher.Verify(command.Password, adminUser.PasswordHash))
        {
            return null;
        }

        var accessToken = tokenGenerator.Generate(adminUser);
        return new AdminLoginResult(
            accessToken.Token,
            accessToken.ExpiresAt,
            ToSummary(adminUser));
    }

    /// <summary>
    /// 在尚未有管理員時建立第一個後台管理員。
    /// </summary>
    public async Task<AdminBootstrapResult> BootstrapAsync(AdminBootstrapCommand command)
    {
        if (await adminUserRepository.HasAnyAsync())
        {
            return new AdminBootstrapResult(false, "Admin user already exists.", null);
        }

        var username = NormalizeUsername(command.Username);
        var displayName = NormalizeDisplayName(command.DisplayName, username);
        if (username is null || command.Password.Length < 12)
        {
            return new AdminBootstrapResult(false, "Invalid username or password.", null);
        }

        var adminUser = await adminUserRepository.CreateAsync(new CreateAdminUserCommand(
            username,
            passwordHasher.Hash(command.Password),
            displayName,
            AdminRole.Admin));

        return new AdminBootstrapResult(true, null, ToSummary(adminUser));
    }

    /// <summary>
    /// 將帳號轉為標準格式。
    /// </summary>
    private static string? NormalizeUsername(string? username)
    {
        return string.IsNullOrWhiteSpace(username) ? null : username.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// 將顯示名稱轉為標準格式。
    /// </summary>
    private static string NormalizeDisplayName(string? displayName, string? username)
    {
        return string.IsNullOrWhiteSpace(displayName) ? username ?? "admin" : displayName.Trim();
    }

    /// <summary>
    /// 將管理員資料轉為對外回傳摘要。
    /// </summary>
    private static AdminUserSummary ToSummary(AdminUser user)
    {
        return new AdminUserSummary(
            user.Id,
            user.Username,
            user.DisplayName,
            user.Role,
            user.IsActive,
            user.CreatedAt);
    }
}
