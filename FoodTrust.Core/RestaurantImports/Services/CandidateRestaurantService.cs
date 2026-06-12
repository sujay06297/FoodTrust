using FoodTrust.Core.RestaurantImports.Interfaces;
using FoodTrust.Core.RestaurantImports.Models;

namespace FoodTrust.Core.RestaurantImports.Services;

public sealed class CandidateRestaurantService(ICandidateRestaurantRepository repository) : ICandidateRestaurantService
{
    public Task<CandidateRestaurantSearchResult> SearchAsync(CandidateRestaurantSearchRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Status) &&
            !CandidateRestaurantStatus.IsValid(request.Status.Trim()))
        {
            throw new ArgumentException("Invalid candidate restaurant status.", nameof(request.Status));
        }

        var normalizedRequest = request with
        {
            Page = Math.Max(1, request.Page),
            PageSize = Math.Clamp(request.PageSize, 1, 200)
        };

        return repository.SearchAsync(normalizedRequest);
    }

    public Task<long?> ApproveAsync(ApproveCandidateRestaurantCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ArgumentException("Restaurant name is required.", nameof(command.Name));
        }

        if (string.IsNullOrWhiteSpace(command.Address))
        {
            throw new ArgumentException("Restaurant address is required.", nameof(command.Address));
        }

        return repository.ApproveAsync(command);
    }

    public Task<bool> RejectAsync(long id)
    {
        return repository.RejectAsync(id);
    }
}
