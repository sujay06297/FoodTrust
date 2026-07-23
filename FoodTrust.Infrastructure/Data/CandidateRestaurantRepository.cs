using Dapper;
using FoodTrust.Core.RestaurantImports.Interfaces;
using FoodTrust.Core.RestaurantImports.Models;
using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Infrastructure.Data;

public sealed class CandidateRestaurantRepository(MySqlConnectionFactory connectionFactory) :
    IRestaurantImportTargetRepository,
    ICandidateRestaurantRepository
{
    public async Task<RestaurantUpsertResult> UpsertRestaurantsAsync(IReadOnlyCollection<RestaurantImportRecord> records)
    {
        if (records.Count == 0)
        {
            return new RestaurantUpsertResult(0, 0, 0);
        }

        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();
        var processedCount = 0;
        var now = DateTimeOffset.UtcNow.UtcDateTime;

        foreach (var record in records)
        {
            await connection.ExecuteAsync("""
                INSERT INTO candidate_restaurants (
                    source_system,
                    source_key,
                    raw_name,
                    raw_address,
                    raw_phone_number,
                    suggested_name,
                    raw_payload,
                    status,
                    created_at,
                    updated_at
                )
                VALUES (
                    @SourceSystem,
                    @SourceKey,
                    @Name,
                    @Address,
                    @PhoneNumber,
                    NULL,
                    @RawPayload,
                    @Status,
                    @Now,
                    @Now
                )
                ON DUPLICATE KEY UPDATE
                    raw_name = IF(status = @Status, VALUES(raw_name), raw_name),
                    raw_address = IF(status = @Status, VALUES(raw_address), raw_address),
                    raw_phone_number = IF(status = @Status, VALUES(raw_phone_number), raw_phone_number),
                    raw_payload = IF(status = @Status, VALUES(raw_payload), raw_payload),
                    updated_at = IF(status = @Status, VALUES(updated_at), updated_at);
                """, new
            {
                record.SourceSystem,
                record.SourceKey,
                record.Name,
                record.Address,
                record.PhoneNumber,
                record.RawPayload,
                Status = CandidateRestaurantStatus.Pending,
                Now = now
            });
            processedCount++;
        }

        return new RestaurantUpsertResult(processedCount, 0, 0);
    }

    public async Task<CandidateRestaurantSearchResult> SearchAsync(CandidateRestaurantSearchRequest request)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var keyword = string.IsNullOrWhiteSpace(request.Keyword)
            ? null
            : $"%{request.Keyword.Trim()}%";
        var status = string.IsNullOrWhiteSpace(request.Status)
            ? null
            : request.Status.Trim();
        var offset = (request.Page - 1) * request.PageSize;

        var totalCount = await connection.ExecuteScalarAsync<int>("""
            SELECT COUNT(*)
            FROM candidate_restaurants
            WHERE (@Status IS NULL OR status = @Status)
              AND (@Keyword IS NULL OR raw_name LIKE @Keyword OR raw_address LIKE @Keyword OR source_key LIKE @Keyword);
            """, new { Status = status, Keyword = keyword });

        var rows = await connection.QueryAsync<CandidateRestaurantRow>("""
            SELECT
                id,
                source_system AS SourceSystem,
                source_key AS SourceKey,
                raw_name AS RawName,
                raw_address AS RawAddress,
                raw_phone_number AS RawPhoneNumber,
                suggested_name AS SuggestedName,
                status,
                linked_restaurant_id AS LinkedRestaurantId,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM candidate_restaurants
            WHERE (@Status IS NULL OR status = @Status)
              AND (@Keyword IS NULL OR raw_name LIKE @Keyword OR raw_address LIKE @Keyword OR source_key LIKE @Keyword)
            ORDER BY updated_at DESC, id DESC
            LIMIT @PageSize OFFSET @Offset;
            """, new
        {
            Status = status,
            Keyword = keyword,
            request.PageSize,
            Offset = offset
        });

        return new CandidateRestaurantSearchResult(
            rows.Select(ToListItem).ToArray(),
            request.Page,
            request.PageSize,
            totalCount);
    }

    public async Task<long?> ApproveAsync(ApproveCandidateRestaurantCommand command)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var candidate = await connection.QuerySingleOrDefaultAsync<CandidateRestaurantRow>("""
            SELECT
                id,
                source_system AS SourceSystem,
                source_key AS SourceKey,
                raw_name AS RawName,
                raw_address AS RawAddress,
                raw_phone_number AS RawPhoneNumber,
                suggested_name AS SuggestedName,
                status,
                linked_restaurant_id AS LinkedRestaurantId,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM candidate_restaurants
            WHERE id = @Id;
            """, new { Id = command.CandidateId }, transaction);

        if (candidate is null)
        {
            return null;
        }

        if (candidate.Status != CandidateRestaurantStatus.Pending)
        {
            await transaction.RollbackAsync();
            return candidate.LinkedRestaurantId;
        }

        var now = DateTimeOffset.UtcNow.UtcDateTime;
        var restaurantId = await connection.ExecuteScalarAsync<long?>("""
            SELECT id
            FROM restaurants
            WHERE name = @Name AND address = @Address
            ORDER BY id
            LIMIT 1;
            """, new
        {
            Name = command.Name.Trim(),
            Address = command.Address.Trim()
        }, transaction);

        if (restaurantId is null)
        {
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
            }, transaction);

            restaurantId = await connection.ExecuteScalarAsync<long>(
                "SELECT LAST_INSERT_ID();",
                transaction: transaction);
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
                @RawName,
                @RawAddress,
                @RawPhoneNumber,
                @RawPayload,
                @Now,
                @Now
            )
            ON DUPLICATE KEY UPDATE
                restaurant_id = @RestaurantId,
                raw_name = @RawName,
                raw_address = @RawAddress,
                raw_phone_number = @RawPhoneNumber,
                raw_payload = @RawPayload,
                updated_at = @Now;
            """, new
        {
            RestaurantId = restaurantId.Value,
            candidate.SourceSystem,
            candidate.SourceKey,
            candidate.RawName,
            candidate.RawAddress,
            candidate.RawPhoneNumber,
            RawPayload = await GetRawPayloadAsync(connection, candidate.Id, transaction),
            Now = now
        }, transaction);

        await connection.ExecuteAsync("""
            UPDATE candidate_restaurants
            SET suggested_name = @Name,
                status = @Status,
                linked_restaurant_id = @RestaurantId,
                updated_at = @Now
            WHERE id = @Id;
            """, new
        {
            Id = candidate.Id,
            Name = command.Name.Trim(),
            Status = CandidateRestaurantStatus.Approved,
            RestaurantId = restaurantId.Value,
            Now = now
        }, transaction);

        await transaction.CommitAsync();
        return restaurantId.Value;
    }

    public async Task<bool> RejectAsync(long id)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var affectedRows = await connection.ExecuteAsync("""
            UPDATE candidate_restaurants
            SET status = @Status,
                updated_at = @UpdatedAt
            WHERE id = @Id
              AND status = @PendingStatus;
            """, new
        {
            Id = id,
            Status = CandidateRestaurantStatus.Rejected,
            PendingStatus = CandidateRestaurantStatus.Pending,
            UpdatedAt = DateTimeOffset.UtcNow.UtcDateTime
        });

        return affectedRows > 0;
    }

    private static async Task<string?> GetRawPayloadAsync(
        MySqlConnector.MySqlConnection connection,
        long id,
        System.Data.Common.DbTransaction transaction)
    {
        return await connection.ExecuteScalarAsync<string?>("""
            SELECT raw_payload
            FROM candidate_restaurants
            WHERE id = @Id;
            """, new { Id = id }, transaction);
    }

    private static CandidateRestaurantListItem ToListItem(CandidateRestaurantRow row)
    {
        return new CandidateRestaurantListItem(
            row.Id,
            row.SourceSystem,
            row.SourceKey,
            row.RawName,
            row.RawAddress,
            row.RawPhoneNumber,
            row.SuggestedName,
            row.Status,
            row.LinkedRestaurantId,
            ToUtcOffset(row.CreatedAt),
            ToUtcOffset(row.UpdatedAt));
    }

    private static DateTimeOffset ToUtcOffset(DateTime value)
    {
        return new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private sealed record CandidateRestaurantRow(
        long Id,
        string SourceSystem,
        string SourceKey,
        string RawName,
        string RawAddress,
        string? RawPhoneNumber,
        string? SuggestedName,
        string Status,
        long? LinkedRestaurantId,
        DateTime CreatedAt,
        DateTime UpdatedAt);
}
