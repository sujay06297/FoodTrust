using Dapper;
using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Infrastructure.Data;

public sealed class DapperRestaurantReviewRepository(MySqlConnectionFactory connectionFactory) : IRestaurantReviewRepository
{
    /// <summary>
    /// 新增舊版單一分數評分，並同步寫入評論表。
    /// </summary>
    public async Task<bool> AddRestaurantRatingAsync(long id, CreateRestaurantRatingCommand command)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var exists = await connection.ExecuteScalarAsync<bool>("""
            SELECT EXISTS (
                SELECT 1
                FROM restaurants
                WHERE id = @Id
            );
            """, new { Id = id }, transaction);

        if (!exists)
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow.UtcDateTime;
        var comment = NormalizeOptional(command.Comment);

        await connection.ExecuteAsync("""
            INSERT INTO restaurant_ratings (restaurant_id, score, review_comment, reviewer_name, created_at)
            VALUES (@RestaurantId, @Score, @Comment, @ReviewerName, @CreatedAt);
            """, new
        {
            RestaurantId = id,
            command.Score,
            Comment = comment,
            ReviewerName = NormalizeOptional(command.ReviewerName),
            CreatedAt = now
        }, transaction);

        await connection.ExecuteAsync("""
            INSERT INTO restaurant_reviews (
                restaurant_id,
                taste_score,
                service_score,
                environment_score,
                value_score,
                revisit_score,
                average_score,
                content,
                reviewer_name,
                status,
                created_at,
                updated_at
            )
            VALUES (
                @RestaurantId,
                @Score,
                @Score,
                @Score,
                @Score,
                @Score,
                @Score,
                @Content,
                @ReviewerName,
                @Status,
                @Now,
                @Now
            );
            """, new
        {
            RestaurantId = id,
            Score = (decimal)command.Score,
            Content = comment is { Length: >= 30 }
                ? comment
                : "Legacy rating without detailed review content.",
            ReviewerName = NormalizeOptional(command.ReviewerName),
            Status = RestaurantReviewStatus.Approved,
            Now = now
        }, transaction);

        await transaction.CommitAsync();

        return true;
    }

    /// <summary>
    /// 新增完整餐廳評論並計算平均分數。
    /// </summary>
    public async Task<bool> AddRestaurantReviewAsync(long id, CreateRestaurantReviewCommand command)
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

        var now = DateTimeOffset.UtcNow.UtcDateTime;
        var averageScore = Math.Round(
            (command.TasteScore +
             command.ServiceScore +
             command.EnvironmentScore +
             command.ValueScore +
             command.RevisitScore) / 5m,
            2,
            MidpointRounding.AwayFromZero);

        await connection.ExecuteAsync("""
            INSERT INTO restaurant_reviews (
                restaurant_id,
                taste_score,
                service_score,
                environment_score,
                value_score,
                revisit_score,
                average_score,
                content,
                reviewer_name,
                visit_date,
                price_per_person,
                dining_type,
                companion_type,
                status,
                created_at,
                updated_at
            )
            VALUES (
                @RestaurantId,
                @TasteScore,
                @ServiceScore,
                @EnvironmentScore,
                @ValueScore,
                @RevisitScore,
                @AverageScore,
                @Content,
                @ReviewerName,
                @VisitDate,
                @PricePerPerson,
                @DiningType,
                @CompanionType,
                @Status,
                @Now,
                @Now
            );
            """, new
        {
            RestaurantId = id,
            command.TasteScore,
            command.ServiceScore,
            command.EnvironmentScore,
            command.ValueScore,
            command.RevisitScore,
            AverageScore = averageScore,
            Content = command.Content.Trim(),
            ReviewerName = NormalizeOptional(command.ReviewerName),
            VisitDate = command.VisitDate?.ToDateTime(TimeOnly.MinValue),
            command.PricePerPerson,
            DiningType = NormalizeOptional(command.DiningType),
            CompanionType = NormalizeOptional(command.CompanionType),
            Status = RestaurantReviewStatus.Approved,
            Now = now
        });

        return true;
    }

    /// <summary>
    /// 取得餐廳已核准的公開評論。
    /// </summary>
    public async Task<IReadOnlyList<RestaurantReviewListItem>> GetRestaurantReviewsAsync(long id, int limit)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var rows = await connection.QueryAsync<RestaurantReviewRow>("""
            SELECT
                id,
                restaurant_id AS RestaurantId,
                taste_score AS TasteScore,
                service_score AS ServiceScore,
                environment_score AS EnvironmentScore,
                value_score AS ValueScore,
                revisit_score AS RevisitScore,
                average_score AS AverageScore,
                content,
                reviewer_name AS ReviewerName,
                visit_date AS VisitDate,
                price_per_person AS PricePerPerson,
                dining_type AS DiningType,
                companion_type AS CompanionType,
                status,
                created_at AS CreatedAt
            FROM restaurant_reviews
            WHERE restaurant_id = @RestaurantId
              AND status = @Status
              AND is_suspicious = FALSE
              AND is_deleted = FALSE
            ORDER BY created_at DESC, id DESC
            LIMIT @Limit;
            """, new
        {
            RestaurantId = id,
            Status = RestaurantReviewStatus.Approved,
            Limit = Math.Clamp(limit, 1, 100)
        });

        return rows.Select(ToReviewListItem).ToArray();
    }

    /// <summary>
    /// 查詢後台評論審核列表。
    /// </summary>
    public async Task<AdminRestaurantReviewSearchResult> SearchReviewsForAdminAsync(AdminRestaurantReviewSearchRequest request)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 200);
        var offset = (page - 1) * pageSize;
        var parameters = new
        {
            request.Status,
            request.IsSuspicious,
            request.IsDeleted,
            Limit = pageSize,
            Offset = offset
        };

        var totalCount = await connection.ExecuteScalarAsync<int>("""
            SELECT COUNT(*)
            FROM restaurant_reviews rr
            WHERE (@Status IS NULL OR rr.status = @Status)
              AND (@IsSuspicious IS NULL OR rr.is_suspicious = @IsSuspicious)
              AND (@IsDeleted IS NULL OR rr.is_deleted = @IsDeleted);
            """, parameters);

        var rows = await connection.QueryAsync<AdminRestaurantReviewRow>("""
            SELECT
                rr.id,
                rr.restaurant_id AS RestaurantId,
                r.name AS RestaurantName,
                rr.taste_score AS TasteScore,
                rr.service_score AS ServiceScore,
                rr.environment_score AS EnvironmentScore,
                rr.value_score AS ValueScore,
                rr.revisit_score AS RevisitScore,
                rr.average_score AS AverageScore,
                rr.content,
                rr.reviewer_name AS ReviewerName,
                rr.visit_date AS VisitDate,
                rr.price_per_person AS PricePerPerson,
                rr.dining_type AS DiningType,
                rr.companion_type AS CompanionType,
                rr.status,
                rr.is_suspicious AS IsSuspicious,
                rr.is_deleted AS IsDeleted,
                rr.created_at AS CreatedAt,
                rr.updated_at AS UpdatedAt
            FROM restaurant_reviews rr
            INNER JOIN restaurants r ON r.id = rr.restaurant_id
            WHERE (@Status IS NULL OR rr.status = @Status)
              AND (@IsSuspicious IS NULL OR rr.is_suspicious = @IsSuspicious)
              AND (@IsDeleted IS NULL OR rr.is_deleted = @IsDeleted)
            ORDER BY rr.created_at DESC, rr.id DESC
            LIMIT @Limit OFFSET @Offset;
            """, parameters);

        return new AdminRestaurantReviewSearchResult(
            rows.Select(ToAdminReviewListItem).ToArray(),
            totalCount,
            page,
            pageSize);
    }

    /// <summary>
    /// 更新評論審核狀態。
    /// </summary>
    public async Task<bool> UpdateReviewStatusAsync(long id, string status, long adminUserId, string? reason)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var oldStatus = await GetReviewStatusForUpdateAsync(connection, transaction, id);
        if (oldStatus is null)
        {
            return false;
        }

        await connection.ExecuteAsync("""
            UPDATE restaurant_reviews
            SET status = @Status,
                updated_at = @UpdatedAt
            WHERE id = @Id;
            """, new
        {
            Id = id,
            Status = status,
            UpdatedAt = DateTimeOffset.UtcNow.UtcDateTime
        }, transaction);

        await AddModerationLogAsync(
            connection,
            transaction,
            id,
            adminUserId,
            ReviewModerationAction.UpdateStatus,
            oldStatus,
            status,
            reason);
        await transaction.CommitAsync();

        return true;
    }

    /// <summary>
    /// 更新評論可疑標記。
    /// </summary>
    public async Task<bool> UpdateReviewSuspiciousAsync(long id, bool isSuspicious, long adminUserId, string? reason)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var oldStatus = await GetReviewStatusForUpdateAsync(connection, transaction, id);
        if (oldStatus is null)
        {
            return false;
        }

        var newStatus = isSuspicious ? RestaurantReviewStatus.Suspicious : oldStatus;
        await connection.ExecuteAsync("""
            UPDATE restaurant_reviews
            SET is_suspicious = @IsSuspicious,
                status = @NewStatus,
                updated_at = @UpdatedAt
            WHERE id = @Id;
            """, new
        {
            Id = id,
            IsSuspicious = isSuspicious,
            NewStatus = newStatus,
            UpdatedAt = DateTimeOffset.UtcNow.UtcDateTime
        }, transaction);

        await AddModerationLogAsync(
            connection,
            transaction,
            id,
            adminUserId,
            ReviewModerationAction.MarkSuspicious,
            oldStatus,
            newStatus,
            reason);
        await transaction.CommitAsync();

        return true;
    }

    /// <summary>
    /// 更新評論刪除標記。
    /// </summary>
    public async Task<bool> UpdateReviewDeletedAsync(long id, bool isDeleted, long adminUserId, string? reason)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var oldStatus = await GetReviewStatusForUpdateAsync(connection, transaction, id);
        if (oldStatus is null)
        {
            return false;
        }

        var newStatus = isDeleted ? RestaurantReviewStatus.Deleted : oldStatus;
        await connection.ExecuteAsync("""
            UPDATE restaurant_reviews
            SET is_deleted = @IsDeleted,
                status = @NewStatus,
                updated_at = @UpdatedAt
            WHERE id = @Id;
            """, new
        {
            Id = id,
            IsDeleted = isDeleted,
            NewStatus = newStatus,
            UpdatedAt = DateTimeOffset.UtcNow.UtcDateTime
        }, transaction);

        await AddModerationLogAsync(
            connection,
            transaction,
            id,
            adminUserId,
            ReviewModerationAction.MarkDeleted,
            oldStatus,
            newStatus,
            reason);
        await transaction.CommitAsync();

        return true;
    }

    /// <summary>
    /// 查詢指定評論的後台審核紀錄。
    /// </summary>
    public async Task<IReadOnlyList<AdminReviewModerationLogListItem>> GetReviewModerationLogsAsync(long id, int limit)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var rows = await connection.QueryAsync<AdminReviewModerationLogRow>("""
            SELECT
                l.id,
                l.review_id AS ReviewId,
                l.admin_user_id AS AdminUserId,
                a.username AS AdminUsername,
                a.display_name AS AdminDisplayName,
                l.action,
                l.old_status AS OldStatus,
                l.new_status AS NewStatus,
                l.reason,
                l.created_at AS CreatedAt
            FROM restaurant_review_moderation_logs l
            INNER JOIN admin_users a ON a.id = l.admin_user_id
            WHERE l.review_id = @ReviewId
            ORDER BY l.created_at DESC, l.id DESC
            LIMIT @Limit;
            """, new
        {
            ReviewId = id,
            Limit = Math.Clamp(limit, 1, 100)
        });

        return rows.Select(ToModerationLogListItem).ToArray();
    }

    /// <summary>
    /// 修剪選填字串，並將空白值正規化為 null。
    /// </summary>
    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>
    /// 將資料庫時間戳視為 UTC 時間。
    /// </summary>
    private static DateTimeOffset ToUtcOffset(DateTime value)
    {
        return new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }

    /// <summary>
    /// 鎖定評論並取得目前狀態。
    /// </summary>
    private static async Task<string?> GetReviewStatusForUpdateAsync(
        MySqlConnector.MySqlConnection connection,
        System.Data.Common.DbTransaction transaction,
        long id)
    {
        return await connection.QuerySingleOrDefaultAsync<string>("""
            SELECT status
            FROM restaurant_reviews
            WHERE id = @Id
            FOR UPDATE;
            """, new { Id = id }, transaction);
    }

    /// <summary>
    /// 新增後台評論審核紀錄。
    /// </summary>
    private static async Task AddModerationLogAsync(
        MySqlConnector.MySqlConnection connection,
        System.Data.Common.DbTransaction transaction,
        long reviewId,
        long adminUserId,
        string action,
        string oldStatus,
        string newStatus,
        string? reason)
    {
        await connection.ExecuteAsync("""
            INSERT INTO restaurant_review_moderation_logs (
                review_id,
                admin_user_id,
                action,
                old_status,
                new_status,
                reason,
                created_at
            )
            VALUES (
                @ReviewId,
                @AdminUserId,
                @Action,
                @OldStatus,
                @NewStatus,
                @Reason,
                @CreatedAt
            );
            """, new
        {
            ReviewId = reviewId,
            AdminUserId = adminUserId,
            Action = action,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            Reason = NormalizeOptional(reason),
            CreatedAt = DateTimeOffset.UtcNow.UtcDateTime
        }, transaction);
    }

    /// <summary>
    /// 將資料庫資料列轉換為評論列表項目。
    /// </summary>
    private static RestaurantReviewListItem ToReviewListItem(RestaurantReviewRow row)
    {
        return new RestaurantReviewListItem(
            row.Id,
            row.RestaurantId,
            row.TasteScore,
            row.ServiceScore,
            row.EnvironmentScore,
            row.ValueScore,
            row.RevisitScore,
            row.AverageScore,
            row.Content,
            row.ReviewerName,
            row.VisitDate is null ? null : DateOnly.FromDateTime(row.VisitDate.Value),
            row.PricePerPerson,
            row.DiningType,
            row.CompanionType,
            row.Status,
            ToUtcOffset(row.CreatedAt));
    }

    /// <summary>
    /// 將資料庫資料列轉換為後台評論審核列表項目。
    /// </summary>
    private static AdminRestaurantReviewListItem ToAdminReviewListItem(AdminRestaurantReviewRow row)
    {
        return new AdminRestaurantReviewListItem(
            row.Id,
            row.RestaurantId,
            row.RestaurantName,
            row.TasteScore,
            row.ServiceScore,
            row.EnvironmentScore,
            row.ValueScore,
            row.RevisitScore,
            row.AverageScore,
            row.Content,
            row.ReviewerName,
            row.VisitDate is null ? null : DateOnly.FromDateTime(row.VisitDate.Value),
            row.PricePerPerson,
            row.DiningType,
            row.CompanionType,
            row.Status,
            row.IsSuspicious,
            row.IsDeleted,
            ToUtcOffset(row.CreatedAt),
            ToUtcOffset(row.UpdatedAt));
    }

    /// <summary>
    /// 將資料庫資料列轉換為後台審核紀錄列表項目。
    /// </summary>
    private static AdminReviewModerationLogListItem ToModerationLogListItem(AdminReviewModerationLogRow row)
    {
        return new AdminReviewModerationLogListItem(
            row.Id,
            row.ReviewId,
            row.AdminUserId,
            row.AdminUsername,
            row.AdminDisplayName,
            row.Action,
            row.OldStatus,
            row.NewStatus,
            row.Reason,
            ToUtcOffset(row.CreatedAt));
    }

    private sealed record RestaurantReviewRow(
        long Id,
        long RestaurantId,
        decimal TasteScore,
        decimal ServiceScore,
        decimal EnvironmentScore,
        decimal ValueScore,
        decimal RevisitScore,
        decimal AverageScore,
        string Content,
        string? ReviewerName,
        DateTime? VisitDate,
        int? PricePerPerson,
        string? DiningType,
        string? CompanionType,
        string Status,
        DateTime CreatedAt);

    private sealed record AdminRestaurantReviewRow(
        long Id,
        long RestaurantId,
        string RestaurantName,
        decimal TasteScore,
        decimal ServiceScore,
        decimal EnvironmentScore,
        decimal ValueScore,
        decimal RevisitScore,
        decimal AverageScore,
        string Content,
        string? ReviewerName,
        DateTime? VisitDate,
        int? PricePerPerson,
        string? DiningType,
        string? CompanionType,
        string Status,
        bool IsSuspicious,
        bool IsDeleted,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    private sealed record AdminReviewModerationLogRow(
        long Id,
        long ReviewId,
        long AdminUserId,
        string AdminUsername,
        string AdminDisplayName,
        string Action,
        string OldStatus,
        string NewStatus,
        string? Reason,
        DateTime CreatedAt);
}
