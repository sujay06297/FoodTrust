using FoodTrust.Core.RestaurantImports.Models;

namespace FoodTrust.Core.RestaurantImports.Interfaces;

public interface IRestaurantImportTargetRepository
{
    Task<RestaurantUpsertResult> UpsertRestaurantsAsync(IReadOnlyCollection<RestaurantImportRecord> records);
}
