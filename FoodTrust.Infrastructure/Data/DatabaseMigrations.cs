namespace FoodTrust.Infrastructure.Data;

public static class DatabaseMigrations
{
    public static IReadOnlyList<DatabaseMigration> All { get; } =
    [
        new DatabaseMigration(
            202605150001,
            "Initial restaurant import and review schema",
            """
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

            CREATE TABLE IF NOT EXISTS restaurant_reviews (
                id BIGINT NOT NULL AUTO_INCREMENT,
                restaurant_id BIGINT NOT NULL,
                taste_score DECIMAL(3,2) NOT NULL,
                service_score DECIMAL(3,2) NOT NULL,
                environment_score DECIMAL(3,2) NOT NULL,
                value_score DECIMAL(3,2) NOT NULL,
                revisit_score DECIMAL(3,2) NOT NULL,
                average_score DECIMAL(3,2) NOT NULL,
                content TEXT NOT NULL,
                reviewer_name VARCHAR(100) NULL,
                visit_date DATE NULL,
                price_per_person INT NULL,
                dining_type VARCHAR(50) NULL,
                companion_type VARCHAR(50) NULL,
                status VARCHAR(50) NOT NULL,
                is_suspicious BOOLEAN NOT NULL DEFAULT FALSE,
                is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
                created_at DATETIME(6) NOT NULL,
                updated_at DATETIME(6) NOT NULL,
                PRIMARY KEY (id),
                INDEX ix_restaurant_reviews_restaurant (restaurant_id),
                INDEX ix_restaurant_reviews_score (average_score),
                CONSTRAINT fk_restaurant_reviews_restaurant
                    FOREIGN KEY (restaurant_id) REFERENCES restaurants(id),
                CONSTRAINT ck_restaurant_reviews_taste_score
                    CHECK (taste_score BETWEEN 1 AND 5),
                CONSTRAINT ck_restaurant_reviews_service_score
                    CHECK (service_score BETWEEN 1 AND 5),
                CONSTRAINT ck_restaurant_reviews_environment_score
                    CHECK (environment_score BETWEEN 1 AND 5),
                CONSTRAINT ck_restaurant_reviews_value_score
                    CHECK (value_score BETWEEN 1 AND 5),
                CONSTRAINT ck_restaurant_reviews_revisit_score
                    CHECK (revisit_score BETWEEN 1 AND 5)
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
            """)
    ];
}
