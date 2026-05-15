using FoodTrust.Core.RestaurantImports.Interfaces;
using FoodTrust.Core.RestaurantImports.Models;

namespace FoodTrust.Core.RestaurantImports.Services;

public sealed class RestaurantImportRunService(IRestaurantImportRunRepository repository) : IRestaurantImportRunService
{
    /// <summary>
    /// 以限制筆數取得近期匯入執行摘要。
    /// </summary>
    public Task<IReadOnlyList<RestaurantImportRunListItem>> GetImportRunsAsync(int limit)
    {
        return repository.GetImportRunsAsync(Math.Clamp(limit, 1, 200));
    }
}
