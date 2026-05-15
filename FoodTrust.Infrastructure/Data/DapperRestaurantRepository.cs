using Dapper;
using FoodTrust.Core.RestaurantImports.Models;
using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Infrastructure.Data;

public sealed class DapperRestaurantRepository(MySqlConnectionFactory connectionFactory) : IRestaurantRepository
{
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

    public async Task<long> CreateRestaurantAsync(CreateRestaurantCommand command)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var now = DateTimeOffset.UtcNow.UtcDateTime;

        await connection.ExecuteAsync("""
            INSERT INTO restaurants (name, address, phone_number, status, created_at, updated_at)
            VALUES (@Name, @Address, @PhoneNumber, @Status, @Now, @Now);
            """, new
        {
            Name = command.Name.Trim(),
            Address = command.Address.Trim(),
            PhoneNumber = NormalizeOptional(command.PhoneNumber),
            Status = RestaurantStatus.PendingReview,
            Now = now
        });

        return await connection.ExecuteScalarAsync<long>("SELECT LAST_INSERT_ID();");
    }

    public async Task<bool> UpdateRestaurantAsync(long id, UpdateRestaurantCommand command)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var affectedRows = await connection.ExecuteAsync("""
            UPDATE restaurants
            SET name = @Name,
                address = @Address,
                phone_number = @PhoneNumber,
                updated_at = @UpdatedAt
            WHERE id = @Id;
            """, new
        {
            Id = id,
            Name = command.Name.Trim(),
            Address = command.Address.Trim(),
            PhoneNumber = NormalizeOptional(command.PhoneNumber),
            UpdatedAt = DateTimeOffset.UtcNow.UtcDateTime
        });

        return affectedRows > 0;
    }

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

    public async Task<bool> AddRestaurantRatingAsync(long id, CreateRestaurantRatingCommand command)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var exists = await connection.ExecuteScalarAsync<bool>("""
            SELECT EXISTS (
                SELECT 1
                FROM restaurants
                WHERE id = @Id
            );
            """, new { Id = id });

        if (!exists)
        {
            return false;
        }

        await connection.ExecuteAsync("""
            INSERT INTO restaurant_ratings (restaurant_id, score, review_comment, reviewer_name, created_at)
            VALUES (@RestaurantId, @Score, @Comment, @ReviewerName, @CreatedAt);
            """, new
        {
            RestaurantId = id,
            command.Score,
            Comment = NormalizeOptional(command.Comment),
            ReviewerName = NormalizeOptional(command.ReviewerName),
            CreatedAt = DateTimeOffset.UtcNow.UtcDateTime
        });

        return true;
    }

    public async Task<IReadOnlyList<RestaurantRankingItem>> GetRestaurantRankingsAsync(int limit)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var rows = await connection.QueryAsync<RestaurantRankingRow>("""
            SELECT
                r.id,
                r.name,
                r.address,
                r.phone_number AS PhoneNumber,
                AVG(rr.score) AS AverageScore,
                COUNT(*) AS RatingCount
            FROM restaurants r
            INNER JOIN restaurant_ratings rr ON rr.restaurant_id = r.id
            WHERE r.status = @Status
            GROUP BY r.id, r.name, r.address, r.phone_number
            ORDER BY AverageScore DESC, RatingCount DESC, r.id DESC
            LIMIT @Limit;
            """, new
        {
            Status = RestaurantStatus.Active,
            Limit = Math.Clamp(limit, 1, 100)
        });

        return rows
            .Select(row => new RestaurantRankingItem(
                row.Id,
                row.Name,
                row.Address,
                row.PhoneNumber,
                Math.Round(row.AverageScore, 2),
                row.RatingCount))
            .ToArray();
    }

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

        var parameters = new
        {
            Keyword = keyword,
            Status = status,
            PageSize = pageSize,
            Offset = offset
        };

        var totalCount = await connection.ExecuteScalarAsync<long>("""
            SELECT COUNT(*)
            FROM restaurants
            WHERE (@Keyword IS NULL OR name LIKE @Keyword OR address LIKE @Keyword OR phone_number LIKE @Keyword)
              AND (@Status IS NULL OR status = @Status);
            """, parameters);

        var restaurants = await connection.QueryAsync<RestaurantRow>("""
            SELECT
                id,
                name,
                address,
                phone_number AS PhoneNumber,
                status,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM restaurants
            WHERE (@Keyword IS NULL OR name LIKE @Keyword OR address LIKE @Keyword OR phone_number LIKE @Keyword)
              AND (@Status IS NULL OR status = @Status)
            ORDER BY id DESC
            LIMIT @PageSize OFFSET @Offset;
            """, parameters);

        var items = restaurants
            .Select(row => new RestaurantListItem(
                row.Id,
                row.Name,
                row.Address,
                row.PhoneNumber,
                row.Status,
                new DateTimeOffset(DateTime.SpecifyKind(row.CreatedAt, DateTimeKind.Utc)),
                new DateTimeOffset(DateTime.SpecifyKind(row.UpdatedAt, DateTimeKind.Utc))))
            .ToArray();

        return new RestaurantSearchResult(items, page, pageSize, totalCount);
    }

    public async Task<RestaurantDetail?> GetRestaurantAsync(long id)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var restaurant = await connection.QuerySingleOrDefaultAsync<RestaurantRow>("""
            SELECT
                id,
                name,
                address,
                phone_number AS PhoneNumber,
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
            restaurant.Address,
            restaurant.PhoneNumber,
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

    private static DateTimeOffset ToUtcOffset(DateTime value)
    {
        return new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private sealed record RestaurantRow(
        long Id,
        string Name,
        string Address,
        string? PhoneNumber,
        string Status,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    private sealed record RestaurantRankingRow(
        long Id,
        string Name,
        string Address,
        string? PhoneNumber,
        double AverageScore,
        int RatingCount);

    private sealed record RestaurantSourceRow(
        string SourceSystem,
        string SourceKey,
        string RawName,
        string RawAddress,
        string? RawPhoneNumber,
        DateTime CreatedAt,
        DateTime UpdatedAt);

}
