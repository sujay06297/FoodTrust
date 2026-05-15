using FoodTrust.Core.RestaurantImports.Models;

namespace FoodTrust.Core.RestaurantImports.Interfaces;

public interface IRestaurantImportRunRepository
{
    /// <summary>
    /// 開始匯入執行並回傳產生的識別碼。
    /// </summary>
    Task<long> StartImportRunAsync(string sourceSystem, string sourceUrl, DateTimeOffset startedAt);

    /// <summary>
    /// 以最終統計數字將匯入執行標記為完成。
    /// </summary>
    Task CompleteImportRunAsync(
        long runId,
        int fetchedCount,
        int importedCount,
        int skippedCount,
        DateTimeOffset finishedAt);

    /// <summary>
    /// 以擷取到的錯誤訊息將匯入執行標記為失敗。
    /// </summary>
    Task FailImportRunAsync(long runId, string errorMessage, DateTimeOffset finishedAt);

    /// <summary>
    /// 取得近期匯入執行摘要。
    /// </summary>
    Task<IReadOnlyList<RestaurantImportRunListItem>> GetImportRunsAsync(int limit);
}
