namespace FoodTrust.Core.RestaurantImports.Interfaces;

public interface IRestaurantImportService
{
    /// <summary>
    /// 從設定的外部來源匯入餐廳資料。
    /// </summary>
    Task ImportAsync(int batchSize, CancellationToken cancellationToken);
}
