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
        if (command.UserId <= 0)
        {
            throw new ArgumentException("Restaurant review user identifier is required.", nameof(command.UserId));
        }

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

        return AddRestaurantReviewCoreAsync(id, command);
    }

    /// <summary>
    /// 檢查會員評論頻率並新增完整餐廳評論。
    /// </summary>
    private async Task<bool> AddRestaurantReviewCoreAsync(long id, CreateRestaurantReviewCommand command)
    {
        var sinceUtc = DateTimeOffset.UtcNow.AddDays(-30).UtcDateTime;
        var hasRecentReview = await repository.HasUserReviewedRestaurantSinceAsync(id, command.UserId, sinceUtc);
        if (hasRecentReview)
        {
            throw new ArgumentException("User can only review the same restaurant once within 30 days.", nameof(command.UserId));
        }

        return await repository.AddRestaurantReviewAsync(id, command);
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
    public Task<bool> UpdateReviewStatusAsync(long id, string status, long adminUserId, string? reason)
    {
        if (!RestaurantReviewStatus.IsValid(status))
        {
            throw new ArgumentException("Invalid restaurant review status.", nameof(status));
        }

        return repository.UpdateReviewStatusAsync(id, status, adminUserId, NormalizeReason(reason));
    }

    /// <summary>
    /// 驗證並批次更新評論審核狀態。
    /// </summary>
    public Task<AdminBatchReviewStatusUpdateResult> BatchUpdateReviewStatusAsync(
        IReadOnlyList<long> ids,
        string status,
        long adminUserId,
        string? reason)
    {
        if (ids.Count == 0)
        {
            throw new ArgumentException("Review identifiers are required.", nameof(ids));
        }

        if (ids.Count > 200)
        {
            throw new ArgumentException("Batch review status update cannot exceed 200 reviews.", nameof(ids));
        }

        if (ids.Any(id => id <= 0))
        {
            throw new ArgumentException("Review identifiers must be positive.", nameof(ids));
        }

        if (!RestaurantReviewStatus.IsValid(status))
        {
            throw new ArgumentException("Invalid restaurant review status.", nameof(status));
        }

        var distinctIds = ids.Distinct().ToArray();
        return repository.BatchUpdateReviewStatusAsync(
            distinctIds,
            status,
            adminUserId,
            NormalizeReason(reason));
    }

    /// <summary>
    /// 更新評論可疑標記。
    /// </summary>
    public Task<bool> UpdateReviewSuspiciousAsync(long id, bool isSuspicious, long adminUserId, string? reason)
    {
        return repository.UpdateReviewSuspiciousAsync(id, isSuspicious, adminUserId, NormalizeReason(reason));
    }

    /// <summary>
    /// 更新評論刪除標記。
    /// </summary>
    public Task<bool> UpdateReviewDeletedAsync(long id, bool isDeleted, long adminUserId, string? reason)
    {
        return repository.UpdateReviewDeletedAsync(id, isDeleted, adminUserId, NormalizeReason(reason));
    }

    /// <summary>
    /// 查詢指定評論的後台審核紀錄。
    /// </summary>
    public Task<IReadOnlyList<AdminReviewModerationLogListItem>> GetReviewModerationLogsAsync(long id, int limit)
    {
        return repository.GetReviewModerationLogsAsync(id, Math.Clamp(limit, 1, 100));
    }

    /// <summary>
    /// 驗證條件並搜尋後台審核紀錄。
    /// </summary>
    public Task<AdminReviewModerationLogSearchResult> SearchReviewModerationLogsAsync(
        AdminReviewModerationLogSearchRequest request)
    {
        if (request.ReviewId is <= 0)
        {
            throw new ArgumentException("Review identifier must be positive.", nameof(request.ReviewId));
        }

        if (request.AdminUserId is <= 0)
        {
            throw new ArgumentException("Admin user identifier must be positive.", nameof(request.AdminUserId));
        }

        if (!string.IsNullOrWhiteSpace(request.Action) && !ReviewModerationAction.IsValid(request.Action.Trim()))
        {
            throw new ArgumentException("Invalid review moderation action.", nameof(request.Action));
        }

        if (request.From is not null && request.To is not null && request.From > request.To)
        {
            throw new ArgumentException("Moderation log search start time cannot be greater than end time.");
        }

        var normalizedRequest = request with
        {
            Action = string.IsNullOrWhiteSpace(request.Action) ? null : request.Action.Trim(),
            Page = Math.Max(1, request.Page),
            PageSize = Math.Clamp(request.PageSize, 1, 200)
        };

        return repository.SearchReviewModerationLogsAsync(normalizedRequest);
    }

    /// <summary>
    /// 驗證並建立評論檢舉。
    /// </summary>
    public Task<bool> CreateReviewReportAsync(long reviewId, CreateReviewReportCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.ReasonType))
        {
            throw new ArgumentException("Review report reason type is required.", nameof(command.ReasonType));
        }

        if (command.Content is { Length: > 1000 })
        {
            throw new ArgumentException("Review report content cannot exceed 1000 characters.", nameof(command.Content));
        }

        var normalizedCommand = command with
        {
            ReasonType = command.ReasonType.Trim(),
            Content = NormalizeReason(command.Content),
            ReporterName = NormalizeReason(command.ReporterName)
        };

        return repository.CreateReviewReportAsync(reviewId, normalizedCommand);
    }

    /// <summary>
    /// 查詢後台評論檢舉列表。
    /// </summary>
    public Task<AdminReviewReportSearchResult> SearchReviewReportsForAdminAsync(AdminReviewReportSearchRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Status) && !ReviewReportStatus.IsValid(request.Status.Trim()))
        {
            throw new ArgumentException("Invalid review report status.", nameof(request.Status));
        }

        var normalizedRequest = request with
        {
            Status = string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim(),
            Page = Math.Max(1, request.Page),
            PageSize = Math.Clamp(request.PageSize, 1, 200)
        };

        return repository.SearchReviewReportsForAdminAsync(normalizedRequest);
    }

    /// <summary>
    /// 驗證並更新評論檢舉處理狀態。
    /// </summary>
    public Task<bool> UpdateReviewReportStatusAsync(long reportId, string status, long adminUserId, string? resolutionNote)
    {
        if (!ReviewReportStatus.IsValid(status))
        {
            throw new ArgumentException("Invalid review report status.", nameof(status));
        }

        return repository.UpdateReviewReportStatusAsync(
            reportId,
            status,
            adminUserId,
            NormalizeReason(resolutionNote));
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

    /// <summary>
    /// 修剪審核原因，並將空白值正規化為 null。
    /// </summary>
    private static string? NormalizeReason(string? reason)
    {
        return string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
    }
}
