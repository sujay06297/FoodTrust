using FoodTrust.Api.Options;
using FoodTrust.Api.Security;
using FoodTrust.Core.Admin.Interfaces;
using FoodTrust.Core.Admin.Services;
using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Services;
using FoodTrust.Core.Users.Interfaces;
using FoodTrust.Core.Users.Services;

namespace FoodTrust.Api;

public static class DependencyInjection
{
    /// <summary>
    /// 註冊 Controller 使用的 API 層應用服務。
    /// </summary>
    public static IServiceCollection AddFoodTrustApiServices(this IServiceCollection services)
    {
        services.AddOptions<AdminJwtOptions>()
            .BindConfiguration(AdminJwtOptions.SectionName)
            .Validate(options => !string.IsNullOrWhiteSpace(options.SigningKey), "AdminJwt:SigningKey is required.")
            .Validate(options => options.SigningKey.Length >= 32, "AdminJwt:SigningKey must be at least 32 characters.")
            .ValidateOnStart();
        services.AddOptions<UserJwtOptions>()
            .BindConfiguration(UserJwtOptions.SectionName)
            .Validate(options => !string.IsNullOrWhiteSpace(options.SigningKey), "UserJwt:SigningKey is required.")
            .Validate(options => options.SigningKey.Length >= 32, "UserJwt:SigningKey must be at least 32 characters.")
            .ValidateOnStart();

        services.AddScoped<IAdminAuthService, AdminAuthService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IAdminTokenGenerator, JwtAdminTokenGenerator>();
        services.AddScoped<IUserAuthService, UserAuthService>();
        services.AddScoped<IUserTokenGenerator, JwtUserTokenGenerator>();
        services.AddScoped<IRestaurantService, RestaurantService>();
        services.AddScoped<IRestaurantReviewService, RestaurantReviewService>();
        services.AddScoped<IRestaurantFavoriteService, RestaurantFavoriteService>();
        services.AddScoped<IRestaurantRankingService, RestaurantRankingService>();

        return services;
    }
}
