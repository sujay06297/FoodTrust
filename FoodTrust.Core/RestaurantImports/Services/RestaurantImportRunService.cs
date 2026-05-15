using FoodTrust.Core.RestaurantImports.Interfaces;
using FoodTrust.Core.RestaurantImports.Models;

namespace FoodTrust.Core.RestaurantImports.Services;

public sealed class RestaurantImportRunService(IRestaurantImportRunRepository repository) : IRestaurantImportRunService
{
    public Task<IReadOnlyList<RestaurantImportRunListItem>> GetImportRunsAsync(int limit)
    {
        return repository.GetImportRunsAsync(Math.Clamp(limit, 1, 200));
    }
}
