using FoodTrust.Core.RestaurantImports.Models;

namespace FoodTrust.Core.RestaurantImports.Interfaces;

public interface IRestaurantImportRunService
{
    Task<IReadOnlyList<RestaurantImportRunListItem>> GetImportRunsAsync(int limit);
}
