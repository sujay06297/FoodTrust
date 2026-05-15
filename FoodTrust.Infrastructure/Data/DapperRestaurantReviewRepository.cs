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
}
