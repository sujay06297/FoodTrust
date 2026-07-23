using FoodTrust.Core.Admin.Interfaces;
using FoodTrust.Core.RestaurantImports.Interfaces;
using FoodTrust.Core.RestaurantImports.Services;
using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Users.Interfaces;
using FoodTrust.Infrastructure.Data;
using FoodTrust.Infrastructure.Importing;
using FoodTrust.Infrastructure.Options;
using FoodTrust.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FoodTrust.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// 註冊基礎設施服務、資料存取實作與外部來源用戶端。
    /// </summary>
    public static IServiceCollection AddFoodTrustInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RestaurantImportOptions>(
            configuration.GetSection(RestaurantImportOptions.SectionName));

        services.AddHttpClient<FdaFoodBusinessClient>();
        services.AddSingleton<MySqlConnectionFactory>();
        services.AddSingleton<DatabaseInitializer>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<IAdminUserRepository, AdminUserRepository>();
        services.AddScoped<IAdminRefreshTokenRepository, AdminRefreshTokenRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRestaurantRepository, RestaurantRepository>();
        services.AddScoped<ICandidateRestaurantRepository, CandidateRestaurantRepository>();
        services.AddScoped<IRestaurantImportTargetRepository, CandidateRestaurantRepository>();
        services.AddScoped<IRestaurantReviewRepository, RestaurantReviewRepository>();
        services.AddScoped<IRestaurantFavoriteRepository, RestaurantFavoriteRepository>();
        services.AddScoped<IRestaurantRankingRepository, RestaurantRankingRepository>();
        services.AddScoped<IRestaurantImportRunRepository, RestaurantImportRunRepository>();
        services.AddScoped<IRestaurantImportService, RestaurantImportService>();
        services.AddScoped<IRestaurantImportRunService, RestaurantImportRunService>();
        services.AddScoped<IRestaurantImportSource>(provider =>
            provider.GetRequiredService<FdaFoodBusinessClient>());

        return services;
    }
}
