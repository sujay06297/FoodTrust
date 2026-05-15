using FoodTrust.Core.RestaurantImports.Models;

namespace FoodTrust.Core.RestaurantImports.Interfaces;

public interface IRestaurantImportSource
{
    string SourceSystem { get; }

    string SourceUrl { get; }

    Task<IReadOnlyList<RestaurantImportRecord>> FetchRestaurantsAsync(CancellationToken cancellationToken);
}
