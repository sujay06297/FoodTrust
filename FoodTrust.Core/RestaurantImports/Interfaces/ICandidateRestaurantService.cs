using FoodTrust.Core.RestaurantImports.Models;

namespace FoodTrust.Core.RestaurantImports.Interfaces;

public interface ICandidateRestaurantService
{
    Task<CandidateRestaurantSearchResult> SearchAsync(CandidateRestaurantSearchRequest request);

    Task<long?> ApproveAsync(ApproveCandidateRestaurantCommand command);

    Task<bool> RejectAsync(long id);
}
