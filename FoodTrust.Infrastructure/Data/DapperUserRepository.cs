using Dapper;
using FoodTrust.Core.Users.Interfaces;
using FoodTrust.Core.Users.Models;

namespace FoodTrust.Infrastructure.Data;

public sealed class DapperUserRepository(MySqlConnectionFactory connectionFactory) : IUserRepository
{
    /// <summary>
    /// 依電子信箱查詢會員。
    /// </summary>
    public async Task<User?> FindByEmailAsync(string email)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var row = await connection.QuerySingleOrDefaultAsync<UserRow>("""
            SELECT
                id,
                email,
                password_hash AS PasswordHash,
                display_name AS DisplayName,
                status,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM users
            WHERE email = @Email;
            """, new { Email = email });

        return row is null ? null : ToUser(row);
    }

    /// <summary>
    /// 建立新的會員。
    /// </summary>
    public async Task<User> CreateAsync(CreateUserCommand command)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var now = DateTimeOffset.UtcNow.UtcDateTime;
        var id = await connection.ExecuteScalarAsync<long>("""
            INSERT INTO users (
                email,
                password_hash,
                display_name,
                status,
                created_at,
                updated_at
            )
            VALUES (
                @Email,
                @PasswordHash,
                @DisplayName,
                @Status,
                @Now,
                @Now
            );
            SELECT LAST_INSERT_ID();
            """, new
        {
            command.Email,
            command.PasswordHash,
            command.DisplayName,
            command.Status,
            Now = now
        });

        return new User(
            id,
            command.Email,
            command.PasswordHash,
            command.DisplayName,
            command.Status,
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
    /// 將資料庫資料列轉換為會員模型。
    /// </summary>
    private static User ToUser(UserRow row)
    {
        return new User(
            row.Id,
            row.Email,
            row.PasswordHash,
            row.DisplayName,
            row.Status,
            ToUtcOffset(row.CreatedAt),
            ToUtcOffset(row.UpdatedAt));
    }

    private sealed record UserRow(
        long Id,
        string Email,
        string PasswordHash,
        string DisplayName,
        string Status,
        DateTime CreatedAt,
        DateTime UpdatedAt);
}
