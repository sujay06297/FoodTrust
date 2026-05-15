using Dapper;
using MySqlConnector;

namespace FoodTrust.Infrastructure.Data;

public sealed class DatabaseInitializer(MySqlConnectionFactory connectionFactory)
{
    public async Task InitializeAsync()
    {
        await EnsureDatabaseAsync();

        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        await connection.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS restaurants (
                id BIGINT NOT NULL AUTO_INCREMENT,
                name VARCHAR(255) NOT NULL,
                address VARCHAR(500) NOT NULL,
                phone_number VARCHAR(50) NULL,
                status VARCHAR(50) NOT NULL DEFAULT 'Active',
                created_at DATETIME(6) NOT NULL,
                updated_at DATETIME(6) NOT NULL,
                PRIMARY KEY (id),
                INDEX ix_restaurants_name (name),
                INDEX ix_restaurants_address (address)
            ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

            CREATE TABLE IF NOT EXISTS restaurant_sources (
                id BIGINT NOT NULL AUTO_INCREMENT,
                restaurant_id BIGINT NOT NULL,
                source_system VARCHAR(100) NOT NULL,
                source_key VARCHAR(128) NOT NULL,
                raw_name VARCHAR(255) NOT NULL,
                raw_address VARCHAR(500) NOT NULL,
                raw_phone_number VARCHAR(50) NULL,
                raw_payload LONGTEXT NULL,
                created_at DATETIME(6) NOT NULL,
                updated_at DATETIME(6) NOT NULL,
                PRIMARY KEY (id),
                CONSTRAINT fk_restaurant_sources_restaurant
                    FOREIGN KEY (restaurant_id) REFERENCES restaurants(id),
                CONSTRAINT ux_restaurant_sources_source
                    UNIQUE (source_system, source_key)
            ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

            CREATE TABLE IF NOT EXISTS restaurant_ratings (
                id BIGINT NOT NULL AUTO_INCREMENT,
                restaurant_id BIGINT NOT NULL,
                score TINYINT NOT NULL,
                review_comment VARCHAR(1000) NULL,
                reviewer_name VARCHAR(100) NULL,
                created_at DATETIME(6) NOT NULL,
                PRIMARY KEY (id),
                INDEX ix_restaurant_ratings_restaurant (restaurant_id),
                CONSTRAINT fk_restaurant_ratings_restaurant
                    FOREIGN KEY (restaurant_id) REFERENCES restaurants(id),
                CONSTRAINT ck_restaurant_ratings_score
                    CHECK (score BETWEEN 1 AND 5)
            ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

            CREATE TABLE IF NOT EXISTS restaurant_import_runs (
                id BIGINT NOT NULL AUTO_INCREMENT,
                source_system VARCHAR(100) NOT NULL,
                source_url VARCHAR(1000) NOT NULL,
                started_at DATETIME(6) NOT NULL,
                finished_at DATETIME(6) NULL,
                status VARCHAR(50) NOT NULL,
                fetched_count INT NOT NULL DEFAULT 0,
                imported_count INT NOT NULL DEFAULT 0,
                skipped_count INT NOT NULL DEFAULT 0,
                error_message TEXT NULL,
                PRIMARY KEY (id)
            ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
            """);
    }

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

    private static string EscapeIdentifier(string identifier)
    {
        return identifier.Replace("`", "``", StringComparison.Ordinal);
    }
}
