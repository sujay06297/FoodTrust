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
                user_id,
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
        var suspicion = await DetectSuspiciousReviewAsync(connection, id, command, averageScore, now);

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
                is_suspicious,
                suspicious_reason,
                suspicious_detected_at,
                created_at,
                updated_at
            )
            VALUES (
                @RestaurantId,
                @UserId,
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
                @IsSuspicious,
                @SuspiciousReason,
                @SuspiciousDetectedAt,
                @Now,
                @Now
            );
            """, new
        {
            RestaurantId = id,
            command.UserId,
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
            Status = suspicion.IsSuspicious ? RestaurantReviewStatus.Suspicious : RestaurantReviewStatus.Approved,
            suspicion.IsSuspicious,
            SuspiciousReason = suspicion.Reason,
            SuspiciousDetectedAt = suspicion.IsSuspicious ? now : (DateTime?)null,
            Now = now
        });

        return true;
    }

    /// <summary>
    /// 判斷會員近期是否已評論同一間餐廳。
    /// </summary>
    public async Task<bool> HasUserReviewedRestaurantSinceAsync(long restaurantId, long userId, DateTime sinceUtc)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        return await connection.ExecuteScalarAsync<bool>("""
            SELECT EXISTS (
                SELECT 1
                FROM restaurant_reviews
                WHERE restaurant_id = @RestaurantId
                  AND user_id = @UserId
                  AND created_at >= @SinceUtc
                  AND is_deleted = FALSE
            );
            """, new
        {
            RestaurantId = restaurantId,
            UserId = userId,
            SinceUtc = sinceUtc
        });
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
                rr.user_id AS UserId,
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
                rr.suspicious_reason AS SuspiciousReason,
                rr.suspicious_detected_at AS SuspiciousDetectedAt,
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

        var newStatus = isSuspicious
            ? RestaurantReviewStatus.Suspicious
            : oldStatus == RestaurantReviewStatus.Suspicious
                ? RestaurantReviewStatus.Approved
                : oldStatus;
        await connection.ExecuteAsync("""
            UPDATE restaurant_reviews
            SET is_suspicious = @IsSuspicious,
                status = @NewStatus,
                suspicious_reason = CASE WHEN @IsSuspicious THEN suspicious_reason ELSE NULL END,
                suspicious_detected_at = CASE WHEN @IsSuspicious THEN suspicious_detected_at ELSE NULL END,
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

        var newStatus = isDeleted
            ? RestaurantReviewStatus.Deleted
            : oldStatus == RestaurantReviewStatus.Deleted
                ? RestaurantReviewStatus.Approved
                : oldStatus;
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
    /// 建立評論檢舉。
    /// </summary>
    public async Task<bool> CreateReviewReportAsync(long reviewId, CreateReviewReportCommand command)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var exists = await connection.ExecuteScalarAsync<bool>("""
            SELECT EXISTS (
                SELECT 1
                FROM restaurant_reviews
                WHERE id = @ReviewId
                  AND is_deleted = FALSE
            );
            """, new { ReviewId = reviewId });

        if (!exists)
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow.UtcDateTime;
        await connection.ExecuteAsync("""
            INSERT INTO restaurant_review_reports (
                review_id,
                reason_type,
                content,
                reporter_name,
                status,
                created_at,
                updated_at
            )
            VALUES (
                @ReviewId,
                @ReasonType,
                @Content,
                @ReporterName,
                @Status,
                @Now,
                @Now
            );
            """, new
        {
            ReviewId = reviewId,
            command.ReasonType,
            Content = NormalizeOptional(command.Content),
            ReporterName = NormalizeOptional(command.ReporterName),
            Status = ReviewReportStatus.Pending,
            Now = now
        });

        return true;
    }

    /// <summary>
    /// 查詢後台評論檢舉列表。
    /// </summary>
    public async Task<AdminReviewReportSearchResult> SearchReviewReportsForAdminAsync(AdminReviewReportSearchRequest request)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 200);
        var offset = (page - 1) * pageSize;
        var parameters = new
        {
            request.Status,
            Limit = pageSize,
            Offset = offset
        };

        var totalCount = await connection.ExecuteScalarAsync<int>("""
            SELECT COUNT(*)
            FROM restaurant_review_reports report
            WHERE (@Status IS NULL OR report.status = @Status);
            """, parameters);

        var rows = await connection.QueryAsync<AdminReviewReportRow>("""
            SELECT
                report.id,
                report.review_id AS ReviewId,
                review.restaurant_id AS RestaurantId,
                restaurant.name AS RestaurantName,
                report.reason_type AS ReasonType,
                report.content,
                report.reporter_name AS ReporterName,
                report.status,
                review.status AS ReviewStatus,
                review.content AS ReviewContent,
                report.resolution_note AS ResolutionNote,
                report.resolved_by_admin_user_id AS ResolvedByAdminUserId,
                admin.username AS ResolvedByAdminUsername,
                report.resolved_at AS ResolvedAt,
                report.created_at AS CreatedAt,
                report.updated_at AS UpdatedAt
            FROM restaurant_review_reports report
            INNER JOIN restaurant_reviews review ON review.id = report.review_id
            INNER JOIN restaurants restaurant ON restaurant.id = review.restaurant_id
            LEFT JOIN admin_users admin ON admin.id = report.resolved_by_admin_user_id
            WHERE (@Status IS NULL OR report.status = @Status)
            ORDER BY report.created_at DESC, report.id DESC
            LIMIT @Limit OFFSET @Offset;
            """, parameters);

        return new AdminReviewReportSearchResult(
            rows.Select(ToReviewReportListItem).ToArray(),
            totalCount,
            page,
            pageSize);
    }

    /// <summary>
    /// 更新評論檢舉處理狀態。
    /// </summary>
    public async Task<bool> UpdateReviewReportStatusAsync(
        long reportId,
        string status,
        long adminUserId,
        string? resolutionNote)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var now = DateTimeOffset.UtcNow.UtcDateTime;
        var resolvedAt = status == ReviewReportStatus.Pending ? (DateTime?)null : now;
        var resolvedByAdminUserId = status == ReviewReportStatus.Pending ? (long?)null : adminUserId;
        var affectedRows = await connection.ExecuteAsync("""
            UPDATE restaurant_review_reports
            SET status = @Status,
                resolution_note = @ResolutionNote,
                resolved_by_admin_user_id = @AdminUserId,
                resolved_at = @ResolvedAt,
                updated_at = @UpdatedAt
            WHERE id = @ReportId;
            """, new
        {
            ReportId = reportId,
            Status = status,
            ResolutionNote = NormalizeOptional(resolutionNote),
            AdminUserId = resolvedByAdminUserId,
            ResolvedAt = resolvedAt,
            UpdatedAt = now
        });

        return affectedRows > 0;
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
    /// 依規則式條件偵測新評論是否可疑。
    /// </summary>
    private static async Task<ReviewSuspicionResult> DetectSuspiciousReviewAsync(
        MySqlConnector.MySqlConnection connection,
        long restaurantId,
        CreateRestaurantReviewCommand command,
        decimal averageScore,
        DateTime now)
    {
        var reasons = new List<string>();
        var normalizedContent = command.Content.Trim().ToLowerInvariant();
        var reviewerName = NormalizeOptional(command.ReviewerName);
        var recentThreshold = now.AddHours(-24);
        var duplicateThreshold = now.AddDays(-30);

        if (reviewerName is not null)
        {
            var recentReviewerCount = await connection.ExecuteScalarAsync<int>("""
                SELECT COUNT(*)
                FROM restaurant_reviews
                WHERE restaurant_id = @RestaurantId
                  AND reviewer_name = @ReviewerName
                  AND created_at >= @RecentThreshold;
                """, new
            {
                RestaurantId = restaurantId,
                ReviewerName = reviewerName,
                RecentThreshold = recentThreshold
            });

            if (recentReviewerCount >= 2)
            {
                reasons.Add("同一評論者 24 小時內對同餐廳留下多筆評論");
            }
        }

        var duplicateContentCount = await connection.ExecuteScalarAsync<int>("""
            SELECT COUNT(*)
            FROM restaurant_reviews
            WHERE restaurant_id = @RestaurantId
              AND LOWER(TRIM(content)) = @NormalizedContent
              AND created_at >= @DuplicateThreshold;
            """, new
        {
            RestaurantId = restaurantId,
            NormalizedContent = normalizedContent,
            DuplicateThreshold = duplicateThreshold
        });

        if (duplicateContentCount > 0)
        {
            reasons.Add("30 天內出現相同評論內容");
        }

        if (IsLowQualityContent(command.Content))
        {
            reasons.Add("評論內容重複字元或資訊量偏低");
        }

        var scoreSignal = await connection.QuerySingleOrDefaultAsync<RestaurantScoreSignal>("""
            SELECT
                COUNT(*) AS ReviewCount,
                AVG(average_score) AS AverageScore
            FROM restaurant_reviews
            WHERE restaurant_id = @RestaurantId
              AND status = @Status
              AND is_suspicious = FALSE
              AND is_deleted = FALSE;
            """, new
        {
            RestaurantId = restaurantId,
            Status = RestaurantReviewStatus.Approved
        });

        if (scoreSignal is { ReviewCount: >= 5, AverageScore: not null } &&
            Math.Abs(averageScore - scoreSignal.AverageScore.Value) >= 2m)
        {
            reasons.Add("分數與餐廳既有平均差距過大");
        }

        return new ReviewSuspicionResult(
            reasons.Count > 0,
            reasons.Count > 0 ? string.Join("；", reasons) : null);
    }

    /// <summary>
    /// 判斷評論文字是否具備低品質特徵。
    /// </summary>
    private static bool IsLowQualityContent(string content)
    {
        var trimmed = content.Trim();
        var distinctCharacters = trimmed
            .Where(character => !char.IsWhiteSpace(character) && !char.IsPunctuation(character))
            .Distinct()
            .Count();

        return distinctCharacters <= 8 || trimmed.Length < 45;
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
            row.UserId,
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
            row.SuspiciousReason,
            row.SuspiciousDetectedAt is null ? null : ToUtcOffset(row.SuspiciousDetectedAt.Value),
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

    /// <summary>
    /// 將資料庫資料列轉換為後台檢舉列表項目。
    /// </summary>
    private static AdminReviewReportListItem ToReviewReportListItem(AdminReviewReportRow row)
    {
        return new AdminReviewReportListItem(
            row.Id,
            row.ReviewId,
            row.RestaurantId,
            row.RestaurantName,
            row.ReasonType,
            row.Content,
            row.ReporterName,
            row.Status,
            row.ReviewStatus,
            row.ReviewContent,
            row.ResolutionNote,
            row.ResolvedByAdminUserId,
            row.ResolvedByAdminUsername,
            row.ResolvedAt is null ? null : ToUtcOffset(row.ResolvedAt.Value),
            ToUtcOffset(row.CreatedAt),
            ToUtcOffset(row.UpdatedAt));
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
        long? UserId,
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
        string? SuspiciousReason,
        DateTime? SuspiciousDetectedAt,
        bool IsDeleted,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    private sealed record ReviewSuspicionResult(
        bool IsSuspicious,
        string? Reason);

    private sealed record RestaurantScoreSignal(
        int ReviewCount,
        decimal? AverageScore);

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

    private sealed record AdminReviewReportRow(
        long Id,
        long ReviewId,
        long RestaurantId,
        string RestaurantName,
        string ReasonType,
        string? Content,
        string? ReporterName,
        string Status,
        string ReviewStatus,
        string ReviewContent,
        string? ResolutionNote,
        long? ResolvedByAdminUserId,
        string? ResolvedByAdminUsername,
        DateTime? ResolvedAt,
        DateTime CreatedAt,
        DateTime UpdatedAt);
}
