using FoodTrust.Core.RestaurantImports.Interfaces;
using FoodTrust.Core.RestaurantImports.Services;
using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Infrastructure.Data;
using FoodTrust.Infrastructure.Importing;
using FoodTrust.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FoodTrust.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFoodTrustInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RestaurantImportOptions>(
            configuration.GetSection(RestaurantImportOptions.SectionName));

        services.AddHttpClient<FdaFoodBusinessClient>();
        services.AddSingleton<MySqlConnectionFactory>();
        services.AddSingleton<DatabaseInitializer>();
        services.AddScoped<IRestaurantRepository, DapperRestaurantRepository>();
        services.AddScoped<IRestaurantImportTargetRepository, DapperRestaurantRepository>();
        services.AddScoped<IRestaurantReviewRepository, DapperRestaurantReviewRepository>();
        services.AddScoped<IRestaurantRankingRepository, DapperRestaurantRankingRepository>();
        services.AddScoped<IRestaurantImportRunRepository, DapperRestaurantImportRunRepository>();
        services.AddScoped<IRestaurantImportService, RestaurantImportService>();
        services.AddScoped<IRestaurantImportRunService, RestaurantImportRunService>();
        services.AddScoped<IRestaurantImportSource>(provider =>
            provider.GetRequiredService<FdaFoodBusinessClient>());

        return services;
    }
}
