using Dapper;
using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Infrastructure.Data;

public sealed class RestaurantRankingRepository(MySqlConnectionFactory connectionFactory) : IRestaurantRankingRepository
{
    private const decimal BayesianMinimumReviewCount = 20m;
    private const decimal BayesianGlobalAverageScore = 3.6m;
    private const decimal FavoriteScoreNormalizationCount = 100m;

    /// <summary>
    /// 依 Bayesian 平台分數與評論數取得餐廳排行。
    /// </summary>
    public async Task<IReadOnlyList<RestaurantRankingItem>> GetRestaurantRankingsAsync(int limit)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var rows = await connection.QueryAsync<RestaurantRankingRow>($"""
            SELECT
                r.id,
                r.name,
                r.address,
                r.phone_number AS PhoneNumber,
                review_stats.raw_average_score AS RawAverageScore,
                review_stats.platform_score AS PlatformScore,
                ((review_stats.platform_score * 0.95) +
                    (LEAST(COALESCE(favorite_stats.favorite_count, 0) / {FavoriteScoreNormalizationCount}, 1) * 5 * 0.05)) AS RankingScore,
                COALESCE(favorite_stats.favorite_count, 0) AS FavoriteCount,
                review_stats.review_count AS ReviewCount
            FROM restaurants r
            INNER JOIN (
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
            ) review_stats ON review_stats.restaurant_id = r.id
            LEFT JOIN (
                SELECT
                    restaurant_id,
                    COUNT(*) AS favorite_count
                FROM favorite_restaurants
                GROUP BY restaurant_id
            ) favorite_stats ON favorite_stats.restaurant_id = r.id
            WHERE r.status = @RestaurantStatus
            ORDER BY RankingScore DESC, ReviewCount DESC, r.id DESC
            LIMIT @Limit;
            """, new
        {
            RestaurantStatus = RestaurantStatus.Active,
            ReviewStatus = RestaurantReviewStatus.Approved,
            MinimumReviewCount = BayesianMinimumReviewCount,
            GlobalAverageScore = BayesianGlobalAverageScore,
            Limit = Math.Clamp(limit, 1, 100)
        });

        return rows
            .Select(row => new RestaurantRankingItem(
                row.Id,
                row.Name,
                row.Address,
                row.PhoneNumber,
                Math.Round(row.RawAverageScore, 2),
                Math.Round(row.PlatformScore, 2),
                Math.Round(row.RankingScore, 4),
                row.FavoriteCount,
                row.ReviewCount))
            .ToArray();
    }

    private sealed record RestaurantRankingRow(
        long Id,
        string Name,
        string Address,
        string? PhoneNumber,
        decimal RawAverageScore,
        decimal PlatformScore,
        decimal RankingScore,
        int FavoriteCount,
        int ReviewCount);
}
