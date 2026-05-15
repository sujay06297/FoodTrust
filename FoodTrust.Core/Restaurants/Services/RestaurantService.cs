using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Core.Restaurants.Services;

public sealed class RestaurantService(IRestaurantRepository repository) : IRestaurantService
{
    public Task<long> CreateRestaurantAsync(CreateRestaurantCommand command)
    {
        ValidateRestaurant(command.Name, command.Address);
        return repository.CreateRestaurantAsync(command);
    }

    public Task<bool> UpdateRestaurantAsync(long id, UpdateRestaurantCommand command)
    {
        ValidateRestaurant(command.Name, command.Address);
        return repository.UpdateRestaurantAsync(id, command);
    }

    public Task<bool> UpdateRestaurantStatusAsync(long id, string status)
    {
        if (!RestaurantStatus.IsValid(status))
        {
            throw new ArgumentException("Invalid restaurant status.", nameof(status));
        }

        return repository.UpdateRestaurantStatusAsync(id, status);
    }

    public Task<RestaurantSearchResult> SearchRestaurantsAsync(RestaurantSearchRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Status) && !RestaurantStatus.IsValid(request.Status.Trim()))
        {
            throw new ArgumentException("Invalid restaurant status.", nameof(request.Status));
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

    private static void ValidateRestaurant(string? name, string? address)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Restaurant name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(address))
        {
            throw new ArgumentException("Restaurant address is required.", nameof(address));
        }
    }

}
