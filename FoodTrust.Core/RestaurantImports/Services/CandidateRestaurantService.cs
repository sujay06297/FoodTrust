using FoodTrust.Core.Common.Domain;
using FoodTrust.Core.RestaurantImports.Domain.ValueObjects;
using FoodTrust.Core.RestaurantImports.Interfaces;
using FoodTrust.Core.RestaurantImports.Models;
using FoodTrust.Core.Restaurants.Domain.ValueObjects;

namespace FoodTrust.Core.RestaurantImports.Services;

public sealed class CandidateRestaurantService(ICandidateRestaurantRepository repository) : ICandidateRestaurantService
{
    public Task<CandidateRestaurantSearchResult> SearchAsync(CandidateRestaurantSearchRequest request)
    {
        var status = string.IsNullOrWhiteSpace(request.Status)
            ? null
            : CandidateRestaurantLifecycleStatus.Create(request.Status).Value;
        var pageRequest = PageRequest.Create(request.Page, request.PageSize);

        var normalizedRequest = request with
        {
            Status = status,
            Page = pageRequest.Page,
            PageSize = pageRequest.PageSize
        };

        return repository.SearchAsync(normalizedRequest);
    }

    public Task<long?> ApproveAsync(ApproveCandidateRestaurantCommand command)
    {
        EntityId.Create(command.CandidateId, nameof(command.CandidateId));
        RestaurantName.Create(command.Name);
        RestaurantAddress.Create(command.Address);
        CandidateRestaurantLifecycleStatus.Create(CandidateRestaurantStatus.Approved);

        return repository.ApproveAsync(command);
    }

    public Task<bool> RejectAsync(long id)
    {
        EntityId.Create(id, nameof(id));
        CandidateRestaurantLifecycleStatus.Create(CandidateRestaurantStatus.Rejected);
        return repository.RejectAsync(id);
    }
}
