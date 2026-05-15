using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Core.Restaurants.Services;

public sealed class RestaurantReviewService(IRestaurantReviewRepository repository) : IRestaurantReviewService
{
    /// <summary>
    /// 驗證並新增舊版單一分數評分。
    /// </summary>
    public Task<bool> AddRestaurantRatingAsync(long id, CreateRestaurantRatingCommand command)
    {
        if (command.Score is < 1 or > 5)
        {
            throw new ArgumentException("Restaurant rating score must be between 1 and 5.", nameof(command.Score));
        }

        return repository.AddRestaurantRatingAsync(id, command);
    }

    /// <summary>
    /// 驗證並新增完整餐廳評論。
    /// </summary>
    public Task<bool> AddRestaurantReviewAsync(long id, CreateRestaurantReviewCommand command)
    {
        ValidateScore(command.TasteScore, nameof(command.TasteScore));
        ValidateScore(command.ServiceScore, nameof(command.ServiceScore));
        ValidateScore(command.EnvironmentScore, nameof(command.EnvironmentScore));
        ValidateScore(command.ValueScore, nameof(command.ValueScore));
        ValidateScore(command.RevisitScore, nameof(command.RevisitScore));

        if (string.IsNullOrWhiteSpace(command.Content) || command.Content.Trim().Length < 30)
        {
            throw new ArgumentException("Restaurant review content must be at least 30 characters.", nameof(command.Content));
        }

        if (command.PricePerPerson is < 0)
        {
            throw new ArgumentException("Price per person cannot be negative.", nameof(command.PricePerPerson));
        }

        return repository.AddRestaurantReviewAsync(id, command);
    }

    /// <summary>
    /// 取得餐廳已核准的公開評論。
    /// </summary>
    public Task<IReadOnlyList<RestaurantReviewListItem>> GetRestaurantReviewsAsync(long id, int limit)
    {
        return repository.GetRestaurantReviewsAsync(id, Math.Clamp(limit, 1, 100));
    }

    /// <summary>
    /// 驗證篩選條件並查詢後台評論審核列表。
    /// </summary>
    public Task<AdminRestaurantReviewSearchResult> SearchReviewsForAdminAsync(AdminRestaurantReviewSearchRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Status) && !RestaurantReviewStatus.IsValid(request.Status.Trim()))
        {
            throw new ArgumentException("Invalid restaurant review status.", nameof(request.Status));
        }

        var normalizedRequest = request with
        {
            Status = string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim(),
            Page = Math.Max(1, request.Page),
            PageSize = Math.Clamp(request.PageSize, 1, 200)
        };

        return repository.SearchReviewsForAdminAsync(normalizedRequest);
    }

    /// <summary>
    /// 驗證並更新評論審核狀態。
    /// </summary>
    public Task<bool> UpdateReviewStatusAsync(long id, string status)
    {
        if (!RestaurantReviewStatus.IsValid(status))
        {
            throw new ArgumentException("Invalid restaurant review status.", nameof(status));
        }

        return repository.UpdateReviewStatusAsync(id, status);
    }

    /// <summary>
    /// 更新評論可疑標記。
    /// </summary>
    public Task<bool> UpdateReviewSuspiciousAsync(long id, bool isSuspicious)
    {
        return repository.UpdateReviewSuspiciousAsync(id, isSuspicious);
    }

    /// <summary>
    /// 更新評論刪除標記。
    /// </summary>
    public Task<bool> UpdateReviewDeletedAsync(long id, bool isDeleted)
    {
        return repository.UpdateReviewDeletedAsync(id, isDeleted);
    }

    /// <summary>
    /// 驗證單一評分類別分數。
    /// </summary>
    private static void ValidateScore(decimal score, string parameterName)
    {
        if (score is < 1m or > 5m)
        {
            throw new ArgumentException("Restaurant review score must be between 1 and 5.", parameterName);
        }
    }
}
