using FoodTrust.Core.Restaurants.Domain;
using FoodTrust.Core.Restaurants.Domain.ValueObjects;
using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Core.Restaurants.Services;

public sealed class RestaurantService(IRestaurantRepository repository) : IRestaurantService
{
    public Task<long> CreateRestaurantAsync(CreateRestaurantCommand command)
    {
        var restaurant = Restaurant.Create(command);
        return repository.CreateRestaurantAsync(restaurant.ToCreateCommand());
    }

    public Task<bool> UpdateRestaurantAsync(long id, UpdateRestaurantCommand command)
    {
        var restaurant = Restaurant.Restore(id, command, RestaurantStatus.PendingReview);
        return repository.UpdateRestaurantAsync(id, restaurant.ToUpdateCommand());
    }

    public Task<bool> UpdateRestaurantStatusAsync(long id, string status)
    {
        var lifecycleStatus = RestaurantLifecycleStatus.Create(status);
        return repository.UpdateRestaurantStatusAsync(id, lifecycleStatus.Value);
    }

    public Task<RestaurantSearchResult> SearchRestaurantsAsync(RestaurantSearchRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Status) && !RestaurantStatus.IsValid(request.Status.Trim()))
        {
            throw new ArgumentException("Invalid restaurant status.", nameof(request.Status));
        }

        PriceRange.Create(request.PriceMin, request.PriceMax);

        if (request.MinScore is < 1m or > 5m)
        {
            throw new ArgumentException("Minimum score must be between 1 and 5.", nameof(request.MinScore));
        }

        if (!RestaurantSortBy.IsValid(request.SortBy))
        {
            throw new ArgumentException("Invalid restaurant sort option.", nameof(request.SortBy));
        }

        var normalizedRequest = request with
        {
            Page = Math.Max(1, request.Page),
            PageSize = Math.Clamp(request.PageSize, 1, 200)
        };

        return repository.SearchRestaurantsAsync(normalizedRequest);
    }

    public Task<RestaurantDetail?> GetRestaurantAsync(long id)
    {
        return repository.GetRestaurantAsync(id);
    }
}
