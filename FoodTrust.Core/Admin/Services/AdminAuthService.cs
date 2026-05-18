using System.Security.Cryptography;
using FoodTrust.Core.Admin.Interfaces;
using FoodTrust.Core.Admin.Models;

namespace FoodTrust.Core.Admin.Services;

public sealed class AdminAuthService(
    IAdminUserRepository adminUserRepository,
    IAdminRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    IAdminTokenGenerator tokenGenerator) : IAdminAuthService
{
    private const int RefreshTokenExpirationDays = 14;
    private const int RefreshTokenByteLength = 64;

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
        return await CreateLoginResultAsync(adminUser, accessToken);
    }

    /// <summary>
    /// 使用 refresh token 輪替後台存取權杖。
    /// </summary>
    public async Task<AdminLoginResult?> RefreshAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var tokenHash = HashToken(refreshToken);
        var storedToken = await refreshTokenRepository.FindByTokenHashAsync(tokenHash);
        if (storedToken is null ||
            storedToken.RevokedAt is not null ||
            storedToken.ExpiresAt <= now)
        {
            return null;
        }

        var adminUser = await adminUserRepository.FindByIdAsync(storedToken.AdminUserId);
        if (adminUser is null || !adminUser.IsActive)
        {
            return null;
        }

        await refreshTokenRepository.RevokeAsync(storedToken.Id, now.UtcDateTime);
        var accessToken = tokenGenerator.Generate(adminUser);
        return await CreateLoginResultAsync(adminUser, accessToken);
    }

    /// <summary>
    /// 撤銷後台 refresh token。
    /// </summary>
    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return false;
        }

        var storedToken = await refreshTokenRepository.FindByTokenHashAsync(HashToken(refreshToken));
        if (storedToken is null || storedToken.RevokedAt is not null)
        {
            return false;
        }

        return await refreshTokenRepository.RevokeAsync(storedToken.Id, DateTimeOffset.UtcNow.UtcDateTime);
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
    /// 建立 access token 與 refresh token 登入結果。
    /// </summary>
    private async Task<AdminLoginResult> CreateLoginResultAsync(AdminUser adminUser, AdminAccessToken accessToken)
    {
        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(RefreshTokenExpirationDays);
        await refreshTokenRepository.CreateAsync(new CreateAdminRefreshTokenCommand(
            adminUser.Id,
            HashToken(refreshToken),
            refreshTokenExpiresAt));

        return new AdminLoginResult(
            accessToken.Token,
            accessToken.ExpiresAt,
            refreshToken,
            refreshTokenExpiresAt,
            ToSummary(adminUser));
    }

    /// <summary>
    /// 產生不可預測的 refresh token。
    /// </summary>
    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(RefreshTokenByteLength));
    }

    /// <summary>
    /// 將 refresh token 雜湊後再儲存。
    /// </summary>
    private static string HashToken(string token)
    {
        return Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));
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
