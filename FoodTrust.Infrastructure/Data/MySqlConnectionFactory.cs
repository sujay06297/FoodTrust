using FoodTrust.Infrastructure.Options;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace FoodTrust.Infrastructure.Data;

public sealed class MySqlConnectionFactory(IOptions<RestaurantImportOptions> options)
{
    public string ConnectionString => options.Value.ConnectionString;

    /// <summary>
    /// 使用設定的連線字串建立新的 MySQL 連線。
    /// </summary>
    public MySqlConnection Create()
    {
        return new MySqlConnection(ConnectionString);
    }
}
