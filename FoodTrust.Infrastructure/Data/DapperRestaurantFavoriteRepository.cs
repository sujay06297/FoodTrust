using Dapper;
using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Infrastructure.Data;

public sealed class DapperRestaurantFavoriteRepository(MySqlConnectionFactory connectionFactory) : IRestaurantFavoriteRepository
{
    private const decimal BayesianMinimumReviewCount = 20m;
    private const decimal BayesianGlobalAverageScore = 3.6m;

    /// <summary>
    /// 新增會員餐廳收藏；若已收藏則不重複建立。
    /// </summary>
    public async Task<bool> AddFavoriteAsync(long userId, long restaurantId)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var exists = await RestaurantExistsAsync(connection, restaurantId);
        if (!exists)
        {
            return false;
        }

        await connection.ExecuteAsync("""
            INSERT IGNORE INTO favorite_restaurants (
                user_id,
                restaurant_id,
                created_at
            )
            VALUES (
                @UserId,
                @RestaurantId,
                @CreatedAt
            );
            """, new
        {
            UserId = userId,
            RestaurantId = restaurantId,
            CreatedAt = DateTimeOffset.UtcNow.UtcDateTime
        });

        return true;
    }

    /// <summary>
    /// 移除會員餐廳收藏。
    /// </summary>
    public async Task<bool> RemoveFavoriteAsync(long userId, long restaurantId)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var exists = await RestaurantExistsAsync(connection, restaurantId);
        if (!exists)
        {
            return false;
        }

        await connection.ExecuteAsync("""
            DELETE FROM favorite_restaurants
            WHERE user_id = @UserId
              AND restaurant_id = @RestaurantId;
            """, new
        {
            UserId = userId,
            RestaurantId = restaurantId
        });

        return true;
    }

    /// <summary>
    /// 查詢會員收藏餐廳列表。
    /// </summary>
    public async Task<FavoriteRestaurantSearchResult> SearchFavoritesAsync(long userId, int page, int pageSize)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync();

        var offset = (page - 1) * pageSize;
        var parameters = new
        {
            UserId = userId,
            ReviewStatus = RestaurantReviewStatus.Approved,
            MinimumReviewCount = BayesianMinimumReviewCount,
            GlobalAverageScore = BayesianGlobalAverageScore,
            PageSize = pageSize,
            Offset = offset
        };

        var totalCount = await connection.ExecuteScalarAsync<long>("""
            SELECT COUNT(*)
            FROM favorite_restaurants
            WHERE user_id = @UserId;
            """, parameters);

        var rows = await connection.QueryAsync<FavoriteRestaurantRow>("""
            SELECT
                restaurants.id AS RestaurantId,
                restaurants.name,
                restaurants.branch_name AS BranchName,
                restaurants.address,
                restaurants.phone_number AS PhoneNumber,
                restaurants.city,
                restaurants.district,
                restaurants.price_min AS PriceMin,
                restaurants.price_max AS PriceMax,
                restaurants.cuisine_type AS CuisineType,
                review_stats.raw_average_score AS RawAverageScore,
                review_stats.platform_score AS PlatformScore,
                COALESCE(review_stats.review_count, 0) AS ReviewCount,
                restaurants.status,
                favorite_restaurants.created_at AS FavoritedAt
            FROM favorite_restaurants
            INNER JOIN restaurants ON restaurants.id = favorite_restaurants.restaurant_id
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
            WHERE favorite_restaurants.user_id = @UserId
            ORDER BY favorite_restaurants.created_at DESC, favorite_restaurants.restaurant_id DESC
            LIMIT @PageSize OFFSET @Offset;
            """, parameters);

        return new FavoriteRestaurantSearchResult(
            rows.Select(ToFavoriteListItem).ToArray(),
            page,
            pageSize,
            totalCount);
    }

    /// <summary>
    /// 判斷餐廳是否存在。
    /// </summary>
    private static async Task<bool> RestaurantExistsAsync(MySqlConnector.MySqlConnection connection, long restaurantId)
    {
        return await connection.ExecuteScalarAsync<bool>("""
            SELECT EXISTS (
                SELECT 1
                FROM restaurants
                WHERE id = @RestaurantId
            );
            """, new { RestaurantId = restaurantId });
    }

    /// <summary>
    /// 將資料列轉換為收藏餐廳列表項目。
    /// </summary>
    private static FavoriteRestaurantListItem ToFavoriteListItem(FavoriteRestaurantRow row)
    {
        return new FavoriteRestaurantListItem(
            row.RestaurantId,
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
            row.ReviewCount,
            row.Status,
            new DateTimeOffset(DateTime.SpecifyKind(row.FavoritedAt, DateTimeKind.Utc)));
    }

    private sealed record FavoriteRestaurantRow(
        long RestaurantId,
        string Name,
        string? BranchName,
        string Address,
        string? PhoneNumber,
        string? City,
        string? District,
        int? PriceMin,
        int? PriceMax,
        string? CuisineType,
        decimal? RawAverageScore,
        decimal? PlatformScore,
        int ReviewCount,
        string Status,
        DateTime FavoritedAt);
}
