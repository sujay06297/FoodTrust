using FoodTrust.Infrastructure.Options;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace FoodTrust.Infrastructure.Data;

public sealed class MySqlConnectionFactory(IOptions<RestaurantImportOptions> options)
{
    private readonly RestaurantImportOptions _options = options.Value;

    public string ConnectionString => BuildConnectionString();

    public MySqlConnection Create()
    {
        return new MySqlConnection(BuildConnectionString());
    }

    public MySqlConnection CreateBootstrapConnection()
    {
        return new MySqlConnection(BuildConnectionString(useBootstrapDatabase: true));
    }

    private string BuildConnectionString(bool useBootstrapDatabase = false)
    {
        var builder = new MySqlConnectionStringBuilder(_options.ConnectionString);

        if (useBootstrapDatabase && !string.IsNullOrWhiteSpace(_options.BootstrapDatabase))
        {
            builder.Database = _options.BootstrapDatabase.Trim();
        }

        if (!string.IsNullOrWhiteSpace(_options.CaCertificateFile))
        {
            var certificatePath = Path.GetFullPath(_options.CaCertificateFile);
            if (!File.Exists(certificatePath))
            {
                throw new InvalidOperationException(
                    $"The configured CA certificate file was not found: {certificatePath}");
            }

            builder.SslCa = certificatePath;
            if (builder.SslMode is MySqlSslMode.Preferred or MySqlSslMode.Required)
            {
                builder.SslMode = MySqlSslMode.VerifyCA;
            }
        }

        if (!string.IsNullOrWhiteSpace(_options.TlsVersion))
        {
            builder["TlsVersion"] = _options.TlsVersion.Trim();
        }

        return builder.ConnectionString;
    }
}
