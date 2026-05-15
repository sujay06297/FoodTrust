using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Core.Restaurants.Services;

public sealed class RestaurantService(IRestaurantRepository repository) : IRestaurantService
{
    /// <summary>
    /// 驗證並建立餐廳。
    /// </summary>
    public Task<long> CreateRestaurantAsync(CreateRestaurantCommand command)
    {
        ValidateRestaurant(command.Name, command.Address);
        ValidatePriceRange(command.PriceMin, command.PriceMax);
        return repository.CreateRestaurantAsync(command);
    }

    /// <summary>
    /// 驗證並更新餐廳。
    /// </summary>
    public Task<bool> UpdateRestaurantAsync(long id, UpdateRestaurantCommand command)
    {
        ValidateRestaurant(command.Name, command.Address);
        ValidatePriceRange(command.PriceMin, command.PriceMax);
        return repository.UpdateRestaurantAsync(id, command);
    }

    /// <summary>
    /// 驗證並更新餐廳狀態。
    /// </summary>
    public Task<bool> UpdateRestaurantStatusAsync(long id, string status)
    {
        if (!RestaurantStatus.IsValid(status))
        {
            throw new ArgumentException("Invalid restaurant status.", nameof(status));
        }

        return repository.UpdateRestaurantStatusAsync(id, status);
    }

    /// <summary>
    /// 驗證篩選條件並查詢餐廳。
    /// </summary>
    public Task<RestaurantSearchResult> SearchRestaurantsAsync(RestaurantSearchRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Status) && !RestaurantStatus.IsValid(request.Status.Trim()))
        {
            throw new ArgumentException("Invalid restaurant status.", nameof(request.Status));
        }

        ValidatePriceRange(request.PriceMin, request.PriceMax);

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

    /// <summary>
    /// 依識別碼取得餐廳詳細資料。
    /// </summary>
    public Task<RestaurantDetail?> GetRestaurantAsync(long id)
    {
        return repository.GetRestaurantAsync(id);
    }

    /// <summary>
    /// 驗證餐廳必要識別欄位。
    /// </summary>
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

    /// <summary>
    /// 驗證選填價格區間不可為負數且順序正確。
    /// </summary>
    private static void ValidatePriceRange(int? priceMin, int? priceMax)
    {
        if (priceMin is < 0 || priceMax is < 0)
        {
            throw new ArgumentException("Restaurant price cannot be negative.");
        }

        if (priceMin is not null && priceMax is not null && priceMin > priceMax)
        {
            throw new ArgumentException("Restaurant price minimum cannot be greater than price maximum.");
        }
    }
}
