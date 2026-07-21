using FoodTrust.Core.Common.Domain;
using FoodTrust.Core.Restaurants.Domain;
using FoodTrust.Core.Restaurants.Domain.ValueObjects;
using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Core.Restaurants.Services;

public sealed class RestaurantReviewService(IRestaurantReviewRepository repository) : IRestaurantReviewService
{
    public Task<bool> AddRestaurantRatingAsync(long id, CreateRestaurantRatingCommand command)
    {
        EntityId.Create(id, nameof(id));
        ReviewScore.Create(command.Score, nameof(command.Score));
        return repository.AddRestaurantRatingAsync(id, command);
    }

    public Task<bool> AddRestaurantReviewAsync(long id, CreateRestaurantReviewCommand command)
    {
        EntityId.Create(id, nameof(id));
        var review = RestaurantReview.Create(id, command);
        return AddRestaurantReviewCoreAsync(review, command);
    }

    private async Task<bool> AddRestaurantReviewCoreAsync(
        RestaurantReview review,
        CreateRestaurantReviewCommand command)
    {
        var sinceUtc = RestaurantReview.RepeatReviewWindowStart(DateTimeOffset.UtcNow);
        var hasRecentReview = await repository.HasUserReviewedRestaurantSinceAsync(
            review.RestaurantId.Value,
            review.UserId.Value,
            sinceUtc);

        if (hasRecentReview)
        {
            throw new ArgumentException("User can only review the same restaurant once within 30 days.", nameof(command.UserId));
        }

        return await repository.AddRestaurantReviewAsync(review.RestaurantId.Value, command);
    }

    public Task<IReadOnlyList<RestaurantReviewListItem>> GetRestaurantReviewsAsync(long id, int limit)
    {
        EntityId.Create(id, nameof(id));
        return repository.GetRestaurantReviewsAsync(id, Math.Clamp(limit, 1, 100));
    }

    public Task<AdminRestaurantReviewSearchResult> SearchReviewsForAdminAsync(AdminRestaurantReviewSearchRequest request)
    {
        var status = string.IsNullOrWhiteSpace(request.Status)
            ? null
            : RestaurantReviewStatusName.Create(request.Status).Value;
        var pageRequest = PageRequest.Create(request.Page, request.PageSize);

        var normalizedRequest = request with
        {
            Status = status,
            Page = pageRequest.Page,
            PageSize = pageRequest.PageSize
        };

        return repository.SearchReviewsForAdminAsync(normalizedRequest);
    }

    public Task<bool> UpdateReviewStatusAsync(long id, string status, long adminUserId, string? reason)
    {
        EntityId.Create(id, nameof(id));
        EntityId.Create(adminUserId, nameof(adminUserId));
        var statusName = RestaurantReviewStatusName.Create(status);
        return repository.UpdateReviewStatusAsync(id, statusName.Value, adminUserId, NormalizeReason(reason));
    }

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

        foreach (var id in ids)
        {
            EntityId.Create(id, nameof(ids));
        }

        EntityId.Create(adminUserId, nameof(adminUserId));
        var statusName = RestaurantReviewStatusName.Create(status);
        var distinctIds = ids.Distinct().ToArray();

        return repository.BatchUpdateReviewStatusAsync(
            distinctIds,
            statusName.Value,
            adminUserId,
            NormalizeReason(reason));
    }

    public Task<bool> UpdateReviewSuspiciousAsync(long id, bool isSuspicious, long adminUserId, string? reason)
    {
        EntityId.Create(id, nameof(id));
        EntityId.Create(adminUserId, nameof(adminUserId));
        return repository.UpdateReviewSuspiciousAsync(id, isSuspicious, adminUserId, NormalizeReason(reason));
    }

    public Task<bool> UpdateReviewDeletedAsync(long id, bool isDeleted, long adminUserId, string? reason)
    {
        EntityId.Create(id, nameof(id));
        EntityId.Create(adminUserId, nameof(adminUserId));
        return repository.UpdateReviewDeletedAsync(id, isDeleted, adminUserId, NormalizeReason(reason));
    }

    public Task<IReadOnlyList<AdminReviewModerationLogListItem>> GetReviewModerationLogsAsync(long id, int limit)
    {
        EntityId.Create(id, nameof(id));
        return repository.GetReviewModerationLogsAsync(id, Math.Clamp(limit, 1, 100));
    }

    public Task<AdminReviewModerationLogSearchResult> SearchReviewModerationLogsAsync(
        AdminReviewModerationLogSearchRequest request)
    {
        if (request.ReviewId is not null)
        {
            EntityId.Create(request.ReviewId.Value, nameof(request.ReviewId));
        }

        if (request.AdminUserId is not null)
        {
            EntityId.Create(request.AdminUserId.Value, nameof(request.AdminUserId));
        }

        var action = ModerationActionName.CreateOptional(request.Action)?.Value;
        if (request.From is not null && request.To is not null && request.From > request.To)
        {
            throw new ArgumentException("Moderation log search start time cannot be greater than end time.");
        }

        var pageRequest = PageRequest.Create(request.Page, request.PageSize);
        var normalizedRequest = request with
        {
            Action = action,
            Page = pageRequest.Page,
            PageSize = pageRequest.PageSize
        };

        return repository.SearchReviewModerationLogsAsync(normalizedRequest);
    }

    public Task<bool> CreateReviewReportAsync(long reviewId, CreateReviewReportCommand command)
    {
        EntityId.Create(reviewId, nameof(reviewId));
        var reason = ReviewReportReason.Create(command.ReasonType);
        var content = OptionalText.Create(command.Content, 1000, nameof(command.Content));
        var reporterName = OptionalText.Create(command.ReporterName, name: nameof(command.ReporterName));

        var normalizedCommand = command with
        {
            ReasonType = reason.Value,
            Content = content.Value,
            ReporterName = reporterName.Value
        };

        return repository.CreateReviewReportAsync(reviewId, normalizedCommand);
    }

    public Task<AdminReviewReportSearchResult> SearchReviewReportsForAdminAsync(AdminReviewReportSearchRequest request)
    {
        var status = string.IsNullOrWhiteSpace(request.Status)
            ? null
            : ReviewReportStatusName.Create(request.Status).Value;
        var pageRequest = PageRequest.Create(request.Page, request.PageSize);

        var normalizedRequest = request with
        {
            Status = status,
            Page = pageRequest.Page,
            PageSize = pageRequest.PageSize
        };

        return repository.SearchReviewReportsForAdminAsync(normalizedRequest);
    }

    public Task<bool> UpdateReviewReportStatusAsync(long reportId, string status, long adminUserId, string? resolutionNote)
    {
        EntityId.Create(reportId, nameof(reportId));
        EntityId.Create(adminUserId, nameof(adminUserId));
        var statusName = ReviewReportStatusName.Create(status);

        return repository.UpdateReviewReportStatusAsync(
            reportId,
            statusName.Value,
            adminUserId,
            NormalizeReason(resolutionNote));
    }

    private static string? NormalizeReason(string? reason)
    {
        return OptionalText.Create(reason).Value;
    }
}
