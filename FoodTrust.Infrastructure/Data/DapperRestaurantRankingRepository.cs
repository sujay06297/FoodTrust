using Dapper;
using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Infrastructure.Data;

public sealed class DapperRestaurantRankingRepository(MySqlConnectionFactory connectionFactory) : IRestaurantRankingRepository
{
    private const decimal BayesianMinimumReviewCount = 20m;
    private const decimal BayesianGlobalAverageScore = 3.6m;

    /// <summary>
    /// 依 Bayesian 平台分數與評論數取得餐廳排行。
    /// </summary>
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
                AVG(rr.average_score) AS RawAverageScore,
                ((COUNT(*) / (COUNT(*) + @MinimumReviewCount)) * AVG(rr.average_score)) +
                    ((@MinimumReviewCount / (COUNT(*) + @MinimumReviewCount)) * @GlobalAverageScore) AS PlatformScore,
                ((COUNT(*) / (COUNT(*) + @MinimumReviewCount)) * AVG(rr.average_score)) +
                    ((@MinimumReviewCount / (COUNT(*) + @MinimumReviewCount)) * @GlobalAverageScore) AS RankingScore,
                COUNT(*) AS ReviewCount
            FROM restaurants r
            INNER JOIN restaurant_reviews rr ON rr.restaurant_id = r.id
            WHERE r.status = @RestaurantStatus
              AND rr.status = @ReviewStatus
              AND rr.is_suspicious = FALSE
              AND rr.is_deleted = FALSE
            GROUP BY r.id, r.name, r.address, r.phone_number
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
        int ReviewCount);
}
