namespace FoodTrust.Core.RestaurantImports.Interfaces;

public interface IRestaurantImportService
{
    Task ImportAsync(int batchSize, CancellationToken cancellationToken);
}
