using Dapper;
using FoodTrust.Core.Admin.Interfaces;
using FoodTrust.Core.Admin.Models;

namespace FoodTrust.Infrastructure.Data;

public sealed class DapperAdminUserRepository(MySqlConnectionFactory connectionFactory) : IAdminUserRepository
{
    /// <summary>
    /// 判斷系統是否已存在任何管理員。
    /// </summary>
    public async Task<bool> HasAnyAsync()
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        return await connection.ExecuteScalarAsync<bool>("""
            SELECT EXISTS (
                SELECT 1
                FROM admin_users
            );
            """);
    }

    /// <summary>
    /// 依帳號查詢管理員。
    /// </summary>
    public async Task<AdminUser?> FindByUsernameAsync(string username)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var row = await connection.QuerySingleOrDefaultAsync<AdminUserRow>("""
            SELECT
                id,
                username,
                password_hash AS PasswordHash,
                display_name AS DisplayName,
                role,
                is_active AS IsActive,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM admin_users
            WHERE username = @Username;
            """, new { Username = username });

        return row is null ? null : ToAdminUser(row);
    }

    /// <summary>
    /// 建立新的後台管理員。
    /// </summary>
    public async Task<AdminUser> CreateAsync(CreateAdminUserCommand command)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var now = DateTimeOffset.UtcNow.UtcDateTime;
        var id = await connection.ExecuteScalarAsync<long>("""
            INSERT INTO admin_users (
                username,
                password_hash,
                display_name,
                role,
                is_active,
                created_at,
                updated_at
            )
            VALUES (
                @Username,
                @PasswordHash,
                @DisplayName,
                @Role,
                TRUE,
                @Now,
                @Now
            );
            SELECT LAST_INSERT_ID();
            """, new
        {
            command.Username,
            command.PasswordHash,
            command.DisplayName,
            command.Role,
            Now = now
        });

        return new AdminUser(
            id,
            command.Username,
            command.PasswordHash,
            command.DisplayName,
            command.Role,
            true,
            ToUtcOffset(now),
            ToUtcOffset(now));
    }

    /// <summary>
    /// 將資料庫時間戳視為 UTC 時間。
    /// </summary>
    private static DateTimeOffset ToUtcOffset(DateTime value)
    {
        return new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }

    /// <summary>
    /// 將資料庫資料列轉換為管理員模型。
    /// </summary>
    private static AdminUser ToAdminUser(AdminUserRow row)
    {
        return new AdminUser(
            row.Id,
            row.Username,
            row.PasswordHash,
            row.DisplayName,
            row.Role,
            row.IsActive,
            ToUtcOffset(row.CreatedAt),
            ToUtcOffset(row.UpdatedAt));
    }

    private sealed record AdminUserRow(
        long Id,
        string Username,
        string PasswordHash,
        string DisplayName,
        string Role,
        bool IsActive,
        DateTime CreatedAt,
        DateTime UpdatedAt);
}
