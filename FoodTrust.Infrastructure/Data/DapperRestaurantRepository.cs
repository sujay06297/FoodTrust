using Dapper;
using FoodTrust.Core.RestaurantImports.Models;
using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Infrastructure.Data;

public sealed class DapperRestaurantRepository(MySqlConnectionFactory connectionFactory) :
    IRestaurantRepository
{
    private const decimal BayesianMinimumReviewCount = 20m;
    private const decimal BayesianGlobalAverageScore = 3.6m;
    private const decimal FavoriteScoreNormalizationCount = 100m;

    /// <summary>
    /// 寫入或更新匯入餐廳資料，並將來源資料連結到餐廳。
    /// </summary>
    public async Task<RestaurantUpsertResult> UpsertRestaurantsAsync(IReadOnlyCollection<RestaurantImportRecord> records)
    {
        if (records.Count == 0)
        {
            return new RestaurantUpsertResult(0, 0, 0);
        }

        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var insertedCount = 0;
        var updatedCount = 0;
        var linkedExistingCount = 0;
        var now = DateTimeOffset.UtcNow.UtcDateTime;

        foreach (var record in records)
        {
            var restaurantId = await connection.ExecuteScalarAsync<long?>("""
                SELECT restaurant_id
                FROM restaurant_sources
                WHERE source_system = @SourceSystem AND source_key = @SourceKey;
                """, record, transaction);

            if (restaurantId is null)
            {
                restaurantId = await connection.ExecuteScalarAsync<long?>("""
                    SELECT id
                    FROM restaurants
                    WHERE name = @Name AND address = @Address
                    ORDER BY id
                    LIMIT 1;
                    """, new
                {
                    record.Name,
                    record.Address
                }, transaction);

                if (restaurantId is null)
                {
                    await connection.ExecuteAsync("""
                        INSERT INTO restaurants (name, address, phone_number, status, created_at, updated_at)
                        VALUES (@Name, @Address, @PhoneNumber, 'Active', @Now, @Now);
                        """, new
                    {
                        record.Name,
                        record.Address,
                        record.PhoneNumber,
                        Now = now
                    }, transaction);

                    restaurantId = await connection.ExecuteScalarAsync<long>(
                        "SELECT LAST_INSERT_ID();",
                        transaction: transaction);
                    insertedCount++;
                }
                else
                {
                    linkedExistingCount++;
                }

                await connection.ExecuteAsync("""
                    INSERT INTO restaurant_sources (
                        restaurant_id,
                        source_system,
                        source_key,
                        raw_name,
                        raw_address,
                        raw_phone_number,
                        raw_payload,
                        created_at,
                        updated_at
                    )
                    VALUES (
                        @RestaurantId,
                        @SourceSystem,
                        @SourceKey,
                        @Name,
                        @Address,
                        @PhoneNumber,
                        @RawPayload,
                        @Now,
                        @Now
                    );
                    """, new
                {
                    RestaurantId = restaurantId,
                    record.SourceSystem,
                    record.SourceKey,
                    record.Name,
                    record.Address,
                    record.PhoneNumber,
                    record.RawPayload,
                    Now = now
                }, transaction);
            }
            else
            {
                await connection.ExecuteAsync("""
                    UPDATE restaurants
                    SET name = @Name,
                        address = @Address,
                        phone_number = COALESCE(@PhoneNumber, phone_number),
                        updated_at = @Now
                    WHERE id = @RestaurantId;

                    UPDATE restaurant_sources
                    SET raw_name = @Name,
                        raw_address = @Address,
                        raw_phone_number = @PhoneNumber,
                        raw_payload = @RawPayload,
                        updated_at = @Now
                    WHERE source_system = @SourceSystem AND source_key = @SourceKey;
                    """, new
                {
                    RestaurantId = restaurantId,
                    record.SourceSystem,
                    record.SourceKey,
                    record.Name,
                    record.Address,
                    record.PhoneNumber,
                    record.RawPayload,
                    Now = now
                }, transaction);
                updatedCount++;
            }
        }

        await transaction.CommitAsync();

        return new RestaurantUpsertResult(insertedCount, updatedCount, linkedExistingCount);
    }

    /// <summary>
    /// 建立餐廳基本資料並回傳產生的識別碼。
    /// </summary>
    public async Task<long> CreateRestaurantAsync(CreateRestaurantCommand command)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var now = DateTimeOffset.UtcNow.UtcDateTime;

        await connection.ExecuteAsync("""
            INSERT INTO restaurants (
                name,
                branch_name,
                address,
                phone_number,
                city,
                district,
                latitude,
                longitude,
                opening_hours,
                price_min,
                price_max,
                cuisine_type,
                tags,
                description,
                official_url,
                google_map_url,
                status,
                created_at,
                updated_at
            )
            VALUES (
                @Name,
                @BranchName,
                @Address,
                @PhoneNumber,
                @City,
                @District,
                @Latitude,
                @Longitude,
                @OpeningHours,
                @PriceMin,
                @PriceMax,
                @CuisineType,
                @Tags,
                @Description,
                @OfficialUrl,
                @GoogleMapUrl,
                @Status,
                @Now,
                @Now
            );
            """, new
        {
            Name = command.Name.Trim(),
            BranchName = NormalizeOptional(command.BranchName),
            Address = command.Address.Trim(),
            PhoneNumber = NormalizeOptional(command.PhoneNumber),
            City = NormalizeOptional(command.City),
            District = NormalizeOptional(command.District),
            command.Latitude,
            command.Longitude,
            OpeningHours = NormalizeOptional(command.OpeningHours),
            command.PriceMin,
            command.PriceMax,
            CuisineType = NormalizeOptional(command.CuisineType),
            Tags = NormalizeOptional(command.Tags),
            Description = NormalizeOptional(command.Description),
            OfficialUrl = NormalizeOptional(command.OfficialUrl),
            GoogleMapUrl = NormalizeOptional(command.GoogleMapUrl),
            Status = RestaurantStatus.PendingReview,
            Now = now
        });

        return await connection.ExecuteScalarAsync<long>("SELECT LAST_INSERT_ID();");
    }

    /// <summary>
    /// 更新餐廳基本資料。
    /// </summary>
    public async Task<bool> UpdateRestaurantAsync(long id, UpdateRestaurantCommand command)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var affectedRows = await connection.ExecuteAsync("""
            UPDATE restaurants
            SET name = @Name,
                branch_name = @BranchName,
                address = @Address,
                phone_number = @PhoneNumber,
                city = @City,
                district = @District,
                latitude = @Latitude,
                longitude = @Longitude,
                opening_hours = @OpeningHours,
                price_min = @PriceMin,
                price_max = @PriceMax,
                cuisine_type = @CuisineType,
                tags = @Tags,
                description = @Description,
                official_url = @OfficialUrl,
                google_map_url = @GoogleMapUrl,
                updated_at = @UpdatedAt
            WHERE id = @Id;
            """, new
        {
            Id = id,
            Name = command.Name.Trim(),
            BranchName = NormalizeOptional(command.BranchName),
            Address = command.Address.Trim(),
            PhoneNumber = NormalizeOptional(command.PhoneNumber),
            City = NormalizeOptional(command.City),
            District = NormalizeOptional(command.District),
            command.Latitude,
            command.Longitude,
            OpeningHours = NormalizeOptional(command.OpeningHours),
            command.PriceMin,
            command.PriceMax,
            CuisineType = NormalizeOptional(command.CuisineType),
            Tags = NormalizeOptional(command.Tags),
            Description = NormalizeOptional(command.Description),
            OfficialUrl = NormalizeOptional(command.OfficialUrl),
            GoogleMapUrl = NormalizeOptional(command.GoogleMapUrl),
            UpdatedAt = DateTimeOffset.UtcNow.UtcDateTime
        });

        return affectedRows > 0;
    }

    /// <summary>
    /// 更新餐廳狀態。
    /// </summary>
    public async Task<bool> UpdateRestaurantStatusAsync(long id, string status)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var affectedRows = await connection.ExecuteAsync("""
            UPDATE restaurants
            SET status = @Status,
                updated_at = @UpdatedAt
            WHERE id = @Id;
            """, new
        {
            Id = id,
            Status = status,
            UpdatedAt = DateTimeOffset.UtcNow.UtcDateTime
        });

        return affectedRows > 0;
    }

    /// <summary>
    /// 使用篩選條件、分數摘要與支援的排序查詢餐廳。
    /// </summary>
    public async Task<RestaurantSearchResult> SearchRestaurantsAsync(RestaurantSearchRequest request)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 200);
        var offset = (page - 1) * pageSize;
        var keyword = string.IsNullOrWhiteSpace(request.Keyword)
            ? null
            : $"%{request.Keyword.Trim()}%";
        var status = string.IsNullOrWhiteSpace(request.Status)
            ? null
            : request.Status.Trim();
        var city = string.IsNullOrWhiteSpace(request.City)
            ? null
            : request.City.Trim();
        var district = string.IsNullOrWhiteSpace(request.District)
            ? null
            : request.District.Trim();
        var cuisineType = string.IsNullOrWhiteSpace(request.CuisineType)
            ? null
            : request.CuisineType.Trim();
        var sortBy = string.IsNullOrWhiteSpace(request.SortBy)
            ? RestaurantSortBy.Latest
            : request.SortBy.Trim();
        var orderBy = GetSearchOrderBy(sortBy);

        var parameters = new
        {
            Keyword = keyword,
            Status = status,
            City = city,
            District = district,
            CuisineType = cuisineType,
            request.PriceMin,
            request.PriceMax,
            request.MinScore,
            ReviewStatus = RestaurantReviewStatus.Approved,
            MinimumReviewCount = BayesianMinimumReviewCount,
            GlobalAverageScore = BayesianGlobalAverageScore,
            PageSize = pageSize,
            Offset = offset
        };

        var totalCount = await connection.ExecuteScalarAsync<long>($"""
            SELECT COUNT(*)
            FROM restaurants
            LEFT JOIN (
                SELECT
                    restaurant_id,
                    AVG(average_score) AS raw_average_score,
                    ((COUNT(*) / (COUNT(*) + @MinimumReviewCount)) * AVG(average_score)) +
                        ((@MinimumReviewCount / (COUNT(*) + @MinimumReviewCount)) * @GlobalAverageScore) AS platform_score,
                    COUNT(*) AS review_count
                FROM restaurant_reviews
                WHERE status = @ReviewStatus
                  AND is_suspicious = FALSE
                  AND is_deleted = FALSE
                GROUP BY restaurant_id
            ) review_stats ON review_stats.restaurant_id = restaurants.id
            WHERE (@Keyword IS NULL OR name LIKE @Keyword OR branch_name LIKE @Keyword OR address LIKE @Keyword OR phone_number LIKE @Keyword OR cuisine_type LIKE @Keyword OR tags LIKE @Keyword)
              AND (@Status IS NULL OR status = @Status)
              AND (@City IS NULL OR city = @City)
              AND (@District IS NULL OR district = @District)
              AND (@CuisineType IS NULL OR cuisine_type = @CuisineType)
              AND (@PriceMin IS NULL OR price_max IS NULL OR price_max >= @PriceMin)
              AND (@PriceMax IS NULL OR price_min IS NULL OR price_min <= @PriceMax)
              AND (@MinScore IS NULL OR review_stats.platform_score >= @MinScore);
            """, parameters);

        var restaurants = await connection.QueryAsync<RestaurantRow>($"""
            SELECT
                restaurants.id,
                restaurants.name,
                restaurants.branch_name AS BranchName,
                restaurants.address,
                restaurants.phone_number AS PhoneNumber,
                restaurants.city,
                restaurants.district,
                restaurants.latitude,
                restaurants.longitude,
                restaurants.opening_hours AS OpeningHours,
                restaurants.price_min AS PriceMin,
                restaurants.price_max AS PriceMax,
                restaurants.cuisine_type AS CuisineType,
                restaurants.tags,
                restaurants.description,
                restaurants.official_url AS OfficialUrl,
                restaurants.google_map_url AS GoogleMapUrl,
                review_stats.raw_average_score AS RawAverageScore,
                review_stats.platform_score AS PlatformScore,
                COALESCE(favorite_stats.favorite_count, 0) AS FavoriteCount,
                COALESCE(review_stats.review_count, 0) AS ReviewCount,
                restaurants.status,
                restaurants.created_at AS CreatedAt,
                restaurants.updated_at AS UpdatedAt
            FROM restaurants
            LEFT JOIN (
                SELECT
                    restaurant_id,
                    AVG(average_score) AS raw_average_score,
                    ((COUNT(*) / (COUNT(*) + @MinimumReviewCount)) * AVG(average_score)) +
                        ((@MinimumReviewCount / (COUNT(*) + @MinimumReviewCount)) * @GlobalAverageScore) AS platform_score,
                    COUNT(*) AS review_count
                FROM restaurant_reviews
                WHERE status = @ReviewStatus
                  AND is_suspicious = FALSE
                  AND is_deleted = FALSE
                GROUP BY restaurant_id
            ) review_stats ON review_stats.restaurant_id = restaurants.id
            LEFT JOIN (
                SELECT
                    restaurant_id,
                    COUNT(*) AS favorite_count
                FROM favorite_restaurants
                GROUP BY restaurant_id
            ) favorite_stats ON favorite_stats.restaurant_id = restaurants.id
            WHERE (@Keyword IS NULL OR restaurants.name LIKE @Keyword OR restaurants.branch_name LIKE @Keyword OR restaurants.address LIKE @Keyword OR restaurants.phone_number LIKE @Keyword OR restaurants.cuisine_type LIKE @Keyword OR restaurants.tags LIKE @Keyword)
              AND (@Status IS NULL OR restaurants.status = @Status)
              AND (@City IS NULL OR restaurants.city = @City)
              AND (@District IS NULL OR restaurants.district = @District)
              AND (@CuisineType IS NULL OR restaurants.cuisine_type = @CuisineType)
              AND (@PriceMin IS NULL OR restaurants.price_max IS NULL OR restaurants.price_max >= @PriceMin)
              AND (@PriceMax IS NULL OR restaurants.price_min IS NULL OR restaurants.price_min <= @PriceMax)
              AND (@MinScore IS NULL OR review_stats.platform_score >= @MinScore)
            ORDER BY {orderBy}
            LIMIT @PageSize OFFSET @Offset;
            """, parameters);

        var items = restaurants
            .Select(row => new RestaurantListItem(
                row.Id,
                row.Name,
                row.BranchName,
                row.Address,
                row.PhoneNumber,
                row.City,
                row.District,
                row.PriceMin,
                row.PriceMax,
                row.CuisineType,
                row.RawAverageScore is null ? null : Math.Round(row.RawAverageScore.Value, 2),
                row.PlatformScore is null ? null : Math.Round(row.PlatformScore.Value, 2),
                row.FavoriteCount,
                row.ReviewCount,
                row.Status,
                new DateTimeOffset(DateTime.SpecifyKind(row.CreatedAt, DateTimeKind.Utc)),
                new DateTimeOffset(DateTime.SpecifyKind(row.UpdatedAt, DateTimeKind.Utc))))
            .ToArray();

        return new RestaurantSearchResult(items, page, pageSize, totalCount);
    }

    /// <summary>
    /// 取得餐廳詳細資料與來源中繼資料。
    /// </summary>
    public async Task<RestaurantDetail?> GetRestaurantAsync(long id)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var restaurant = await connection.QuerySingleOrDefaultAsync<RestaurantRow>("""
            SELECT
                id,
                name,
                branch_name AS BranchName,
                address,
                phone_number AS PhoneNumber,
                city,
                district,
                latitude,
                longitude,
                opening_hours AS OpeningHours,
                price_min AS PriceMin,
                price_max AS PriceMax,
                cuisine_type AS CuisineType,
                tags,
                description,
                official_url AS OfficialUrl,
                google_map_url AS GoogleMapUrl,
                NULL AS RawAverageScore,
                NULL AS PlatformScore,
                0 AS FavoriteCount,
                0 AS ReviewCount,
                status,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM restaurants
            WHERE id = @Id;
            """, new { Id = id });

        if (restaurant is null)
        {
            return null;
        }

        var sources = await connection.QueryAsync<RestaurantSourceRow>("""
            SELECT
                source_system AS SourceSystem,
                source_key AS SourceKey,
                raw_name AS RawName,
                raw_address AS RawAddress,
                raw_phone_number AS RawPhoneNumber,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM restaurant_sources
            WHERE restaurant_id = @Id
            ORDER BY id;
            """, new { Id = id });

        return new RestaurantDetail(
            restaurant.Id,
            restaurant.Name,
            restaurant.BranchName,
            restaurant.Address,
            restaurant.PhoneNumber,
            restaurant.City,
            restaurant.District,
            restaurant.Latitude,
            restaurant.Longitude,
            restaurant.OpeningHours,
            restaurant.PriceMin,
            restaurant.PriceMax,
            restaurant.CuisineType,
            restaurant.Tags,
            restaurant.Description,
            restaurant.OfficialUrl,
            restaurant.GoogleMapUrl,
            restaurant.Status,
            ToUtcOffset(restaurant.CreatedAt),
            ToUtcOffset(restaurant.UpdatedAt),
            sources
                .Select(source => new RestaurantSourceDetail(
                    source.SourceSystem,
                    source.SourceKey,
                    source.RawName,
                    source.RawAddress,
                    source.RawPhoneNumber,
                    ToUtcOffset(source.CreatedAt),
                    ToUtcOffset(source.UpdatedAt)))
                .ToArray());
    }

    /// <summary>
    /// 將資料庫時間戳視為 UTC 時間。
    /// </summary>
    private static DateTimeOffset ToUtcOffset(DateTime value)
    {
        return new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }

    /// <summary>
    /// 修剪選填字串，並將空白值正規化為 null。
    /// </summary>
    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private sealed record RestaurantRow(
        long Id,
        string Name,
        string? BranchName,
        string Address,
        string? PhoneNumber,
        string? City,
        string? District,
        decimal? Latitude,
        decimal? Longitude,
        string? OpeningHours,
        int? PriceMin,
        int? PriceMax,
        string? CuisineType,
        string? Tags,
        string? Description,
        string? OfficialUrl,
        string? GoogleMapUrl,
        decimal? RawAverageScore,
        decimal? PlatformScore,
        int FavoriteCount,
        int ReviewCount,
        string Status,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    private sealed record RestaurantSourceRow(
        string SourceSystem,
        string SourceKey,
        string RawName,
        string RawAddress,
        string? RawPhoneNumber,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    /// <summary>
    /// 將支援的搜尋排序選項對應為安全的 SQL ORDER BY 子句。
    /// </summary>
    private static string GetSearchOrderBy(string sortBy)
    {
        return sortBy switch
        {
            RestaurantSortBy.Ranking => $"((COALESCE(review_stats.platform_score, 0) * 0.95) + (LEAST(COALESCE(favorite_stats.favorite_count, 0) / {FavoriteScoreNormalizationCount}, 1) * 5 * 0.05)) DESC, review_stats.review_count DESC, favorite_stats.favorite_count DESC, restaurants.id DESC",
            RestaurantSortBy.ReviewCount => "review_stats.review_count DESC, review_stats.platform_score DESC, restaurants.id DESC",
            RestaurantSortBy.FavoriteCount => "favorite_stats.favorite_count DESC, review_stats.platform_score DESC, restaurants.id DESC",
            _ => "restaurants.id DESC"
        };
    }
}
