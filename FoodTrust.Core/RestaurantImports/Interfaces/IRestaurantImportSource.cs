using FoodTrust.Core.RestaurantImports.Models;

namespace FoodTrust.Core.RestaurantImports.Interfaces;

public interface IRestaurantImportSource
{
    string SourceSystem { get; }

    string SourceUrl { get; }

    /// <summary>
    /// 從外部來源取得餐廳匯入資料。
    /// </summary>
    Task<IReadOnlyList<RestaurantImportRecord>> FetchRestaurantsAsync(CancellationToken cancellationToken);
}
