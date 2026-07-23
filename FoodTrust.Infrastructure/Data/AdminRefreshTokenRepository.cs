using Dapper;
using FoodTrust.Core.Admin.Interfaces;
using FoodTrust.Core.Admin.Models;

namespace FoodTrust.Infrastructure.Data;

public sealed class AdminRefreshTokenRepository(MySqlConnectionFactory connectionFactory) : IAdminRefreshTokenRepository
{
    /// <summary>
    /// 建立後台 refresh token 紀錄。
    /// </summary>
    public async Task<AdminRefreshToken> CreateAsync(CreateAdminRefreshTokenCommand command)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var now = DateTimeOffset.UtcNow.UtcDateTime;
        var id = await connection.ExecuteScalarAsync<long>("""
            INSERT INTO admin_refresh_tokens (
                admin_user_id,
                token_hash,
                expires_at,
                created_at
            )
            VALUES (
                @AdminUserId,
                @TokenHash,
                @ExpiresAt,
                @CreatedAt
            );
            SELECT LAST_INSERT_ID();
            """, new
        {
            command.AdminUserId,
            command.TokenHash,
            ExpiresAt = command.ExpiresAt.UtcDateTime,
            CreatedAt = now
        });

        return new AdminRefreshToken(
            id,
            command.AdminUserId,
            command.TokenHash,
            command.ExpiresAt,
            null,
            new DateTimeOffset(DateTime.SpecifyKind(now, DateTimeKind.Utc)));
    }

    /// <summary>
    /// 依 token hash 查詢 refresh token。
    /// </summary>
    public async Task<AdminRefreshToken?> FindByTokenHashAsync(string tokenHash)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var row = await connection.QuerySingleOrDefaultAsync<AdminRefreshTokenRow>("""
            SELECT
                id,
                admin_user_id AS AdminUserId,
                token_hash AS TokenHash,
                expires_at AS ExpiresAt,
                revoked_at AS RevokedAt,
                created_at AS CreatedAt
            FROM admin_refresh_tokens
            WHERE token_hash = @TokenHash;
            """, new { TokenHash = tokenHash });

        return row is null ? null : ToRefreshToken(row);
    }

    /// <summary>
    /// 撤銷 refresh token。
    /// </summary>
    public async Task<bool> RevokeAsync(long id, DateTime revokedAtUtc)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var affectedRows = await connection.ExecuteAsync("""
            UPDATE admin_refresh_tokens
            SET revoked_at = @RevokedAt
            WHERE id = @Id
              AND revoked_at IS NULL;
            """, new
        {
            Id = id,
            RevokedAt = revokedAtUtc
        });

        return affectedRows > 0;
    }

    /// <summary>
    /// 將資料列轉換為 refresh token 模型。
    /// </summary>
    private static AdminRefreshToken ToRefreshToken(AdminRefreshTokenRow row)
    {
        return new AdminRefreshToken(
            row.Id,
            row.AdminUserId,
            row.TokenHash,
            ToUtcOffset(row.ExpiresAt),
            row.RevokedAt is null ? null : ToUtcOffset(row.RevokedAt.Value),
            ToUtcOffset(row.CreatedAt));
    }

    /// <summary>
    /// 將資料庫時間戳視為 UTC 時間。
    /// </summary>
    private static DateTimeOffset ToUtcOffset(DateTime value)
    {
        return new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }

    private sealed record AdminRefreshTokenRow(
        long Id,
        long AdminUserId,
        string TokenHash,
        DateTime ExpiresAt,
        DateTime? RevokedAt,
        DateTime CreatedAt);
}
