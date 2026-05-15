using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Services;

namespace FoodTrust.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddFoodTrustApiServices(this IServiceCollection services)
    {
        services.AddScoped<IRestaurantService, RestaurantService>();
        services.AddScoped<IRestaurantReviewService, RestaurantReviewService>();
        services.AddScoped<IRestaurantRankingService, RestaurantRankingService>();

        return services;
    }
}
