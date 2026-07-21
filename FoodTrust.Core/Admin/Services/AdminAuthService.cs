using System.Security.Cryptography;
using FoodTrust.Core.Admin.Domain.ValueObjects;
using FoodTrust.Core.Admin.Interfaces;
using FoodTrust.Core.Admin.Models;
using FoodTrust.Core.Users.Domain.ValueObjects;

namespace FoodTrust.Core.Admin.Services;

public sealed class AdminAuthService(
    IAdminUserRepository adminUserRepository,
    IAdminRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    IAdminTokenGenerator tokenGenerator) : IAdminAuthService
{
    private const int RefreshTokenExpirationDays = 14;
    private const int RefreshTokenByteLength = 64;

    public async Task<AdminLoginResult?> LoginAsync(AdminLoginCommand command)
    {
        var username = AdminUsername.NormalizeForLogin(command.Username);
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

    public async Task<AdminBootstrapResult> BootstrapAsync(AdminBootstrapCommand command)
    {
        if (await adminUserRepository.HasAnyAsync())
        {
            return new AdminBootstrapResult(false, "Admin user already exists.", null);
        }

        AdminUsername username;
        AccountPassword password;
        AdminDisplayName displayName;
        try
        {
            username = AdminUsername.Create(command.Username);
            password = AccountPassword.Create(command.Password, nameof(command.Password));
            displayName = AdminDisplayName.Create(command.DisplayName, username);
        }
        catch (ArgumentException)
        {
            return new AdminBootstrapResult(false, "Invalid username or password.", null);
        }

        var adminUser = await adminUserRepository.CreateAsync(new CreateAdminUserCommand(
            username.Value,
            passwordHasher.Hash(password.Value),
            displayName.Value,
            AdminRole.Admin));

        return new AdminBootstrapResult(true, null, ToSummary(adminUser));
    }

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

    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(RefreshTokenByteLength));
    }

    private static string HashToken(string token)
    {
        return Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));
    }

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
