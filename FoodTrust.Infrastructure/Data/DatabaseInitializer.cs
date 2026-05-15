using Dapper;
using MySqlConnector;

namespace FoodTrust.Infrastructure.Data;

public sealed class DatabaseInitializer(MySqlConnectionFactory connectionFactory)
{
    /// <summary>
    /// 確保資料庫存在並套用尚未執行的 schema migration。
    /// </summary>
    public async Task InitializeAsync()
    {
        await EnsureDatabaseAsync();

        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        await EnsureMigrationTableAsync(connection);
        await ApplyPendingMigrationsAsync(connection);
    }

    /// <summary>
    /// 在 migration 追蹤表不存在時建立該資料表。
    /// </summary>
    private static async Task EnsureMigrationTableAsync(MySqlConnection connection)
    {
        await connection.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS schema_migrations (
                version BIGINT NOT NULL,
                name VARCHAR(255) NOT NULL,
                applied_at DATETIME(6) NOT NULL,
                PRIMARY KEY (version)
            ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
            """);
    }

    /// <summary>
    /// 套用尚未記錄在 migration 追蹤表中的 migration。
    /// </summary>
    private static async Task ApplyPendingMigrationsAsync(MySqlConnection connection)
    {
        var appliedVersions = (await connection.QueryAsync<long>(
            "SELECT version FROM schema_migrations;")).ToHashSet();

        foreach (var migration in DatabaseMigrations.All)
        {
            if (appliedVersions.Contains(migration.Version))
            {
                continue;
            }

            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                await connection.ExecuteAsync(migration.Sql, transaction: transaction);
                await connection.ExecuteAsync("""
                    INSERT INTO schema_migrations (version, name, applied_at)
                    VALUES (@Version, @Name, @AppliedAt);
                    """, new
                {
                    migration.Version,
                    migration.Name,
                    AppliedAt = DateTimeOffset.UtcNow.UtcDateTime
                }, transaction);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    /// <summary>
    /// 在開啟應用程式資料庫連線前建立設定指定的資料庫。
    /// </summary>
    private async Task EnsureDatabaseAsync()
    {
        var connectionStringBuilder = new MySqlConnectionStringBuilder(
            connectionFactory.ConnectionString);

        if (string.IsNullOrWhiteSpace(connectionStringBuilder.Database))
        {
            return;
        }

        var database = connectionStringBuilder.Database;
        connectionStringBuilder.Database = string.Empty;

        await using var connection = new MySqlConnection(connectionStringBuilder.ConnectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            $"CREATE DATABASE IF NOT EXISTS `{EscapeIdentifier(database)}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");
    }

    /// <summary>
    /// 逸出 MySQL 識別名稱以便用於反引號識別字。
    /// </summary>
    private static string EscapeIdentifier(string identifier)
    {
        return identifier.Replace("`", "``", StringComparison.Ordinal);
    }
}
