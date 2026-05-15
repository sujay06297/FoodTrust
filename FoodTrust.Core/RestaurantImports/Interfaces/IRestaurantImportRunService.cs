using FoodTrust.Core.RestaurantImports.Models;

namespace FoodTrust.Core.RestaurantImports.Interfaces;

public interface IRestaurantImportRunService
{
    /// <summary>
    /// 取得近期匯入執行摘要。
    /// </summary>
    Task<IReadOnlyList<RestaurantImportRunListItem>> GetImportRunsAsync(int limit);
}
