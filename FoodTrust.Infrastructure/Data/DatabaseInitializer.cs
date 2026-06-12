using Dapper;
using FoodTrust.Infrastructure.Options;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace FoodTrust.Infrastructure.Data;

public sealed class DatabaseInitializer(
    MySqlConnectionFactory connectionFactory,
    IOptions<RestaurantImportOptions> options)
{
    public async Task InitializeAsync()
    {
        await EnsureDatabaseAsync();

        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        await EnsureMigrationTableAsync(connection);
        await ApplyPendingMigrationsAsync(connection);
    }

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

    private async Task EnsureDatabaseAsync()
    {
        if (!options.Value.EnsureDatabase)
        {
            return;
        }

        var connectionStringBuilder = new MySqlConnectionStringBuilder(connectionFactory.ConnectionString);
        if (string.IsNullOrWhiteSpace(connectionStringBuilder.Database))
        {
            return;
        }

        var database = connectionStringBuilder.Database;

        await using var connection = connectionFactory.CreateBootstrapConnection();
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            $"CREATE DATABASE IF NOT EXISTS `{EscapeIdentifier(database)}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");
    }

    private static string EscapeIdentifier(string identifier)
    {
        return identifier.Replace("`", "``", StringComparison.Ordinal);
    }
}
