using FoodTrust.Infrastructure.Options;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace FoodTrust.Infrastructure.Data;

public sealed class MySqlConnectionFactory(IOptions<RestaurantImportOptions> options)
{
    public string ConnectionString => options.Value.ConnectionString;

    public MySqlConnection Create()
    {
        return new MySqlConnection(ConnectionString);
    }
}
