using FoodTrust.Core.RestaurantImports.Models;

namespace FoodTrust.Core.RestaurantImports.Interfaces;

public interface IRestaurantImportTargetRepository
{
    /// <summary>
    /// 將匯入餐廳資料寫入或更新至本地餐廳資料庫。
    /// </summary>
    Task<RestaurantUpsertResult> UpsertRestaurantsAsync(IReadOnlyCollection<RestaurantImportRecord> records);
}
