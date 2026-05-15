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
            """),
        new DatabaseMigration(
            202605150002,
            "Add restaurant profile fields",
            """
            ALTER TABLE restaurants
                ADD COLUMN branch_name VARCHAR(100) NULL AFTER name,
                ADD COLUMN city VARCHAR(100) NULL AFTER phone_number,
                ADD COLUMN district VARCHAR(100) NULL AFTER city,
                ADD COLUMN latitude DECIMAL(10, 7) NULL AFTER district,
                ADD COLUMN longitude DECIMAL(10, 7) NULL AFTER latitude,
                ADD COLUMN opening_hours VARCHAR(1000) NULL AFTER longitude,
                ADD COLUMN price_min INT NULL AFTER opening_hours,
                ADD COLUMN price_max INT NULL AFTER price_min,
                ADD COLUMN cuisine_type VARCHAR(100) NULL AFTER price_max,
                ADD COLUMN tags VARCHAR(500) NULL AFTER cuisine_type,
                ADD COLUMN description TEXT NULL AFTER tags,
                ADD COLUMN official_url VARCHAR(500) NULL AFTER description,
                ADD COLUMN google_map_url VARCHAR(500) NULL AFTER official_url;

            CREATE INDEX ix_restaurants_city_district ON restaurants (city, district);
            CREATE INDEX ix_restaurants_cuisine_type ON restaurants (cuisine_type);
            CREATE INDEX ix_restaurants_price ON restaurants (price_min, price_max);
            """),
        new DatabaseMigration(
            202605150003,
            "Add admin users",
            """
            CREATE TABLE IF NOT EXISTS admin_users (
                id BIGINT NOT NULL AUTO_INCREMENT,
                username VARCHAR(100) NOT NULL,
                password_hash VARCHAR(500) NOT NULL,
                display_name VARCHAR(100) NOT NULL,
                role VARCHAR(50) NOT NULL,
                is_active BOOLEAN NOT NULL DEFAULT TRUE,
                created_at DATETIME(6) NOT NULL,
                updated_at DATETIME(6) NOT NULL,
                PRIMARY KEY (id),
                CONSTRAINT ux_admin_users_username UNIQUE (username)
            ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
            """),
        new DatabaseMigration(
            202605150004,
            "Add restaurant review moderation logs",
            """
            CREATE TABLE IF NOT EXISTS restaurant_review_moderation_logs (
                id BIGINT NOT NULL AUTO_INCREMENT,
                review_id BIGINT NOT NULL,
                admin_user_id BIGINT NOT NULL,
                action VARCHAR(50) NOT NULL,
                old_status VARCHAR(50) NOT NULL,
                new_status VARCHAR(50) NOT NULL,
                reason VARCHAR(500) NULL,
                created_at DATETIME(6) NOT NULL,
                PRIMARY KEY (id),
                INDEX ix_review_moderation_logs_review (review_id, created_at),
                INDEX ix_review_moderation_logs_admin (admin_user_id, created_at),
                CONSTRAINT fk_review_moderation_logs_review
                    FOREIGN KEY (review_id) REFERENCES restaurant_reviews(id),
                CONSTRAINT fk_review_moderation_logs_admin
                    FOREIGN KEY (admin_user_id) REFERENCES admin_users(id)
            ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
            """)
    ];
}
