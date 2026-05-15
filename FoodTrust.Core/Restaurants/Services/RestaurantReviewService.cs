using FoodTrust.Core.Restaurants.Interfaces;
using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Core.Restaurants.Services;

public sealed class RestaurantReviewService(IRestaurantReviewRepository repository) : IRestaurantReviewService
{
    public Task<bool> AddRestaurantRatingAsync(long id, CreateRestaurantRatingCommand command)
    {
        if (command.Score is < 1 or > 5)
        {
            throw new ArgumentException("Restaurant rating score must be between 1 and 5.", nameof(command.Score));
        }

        return repository.AddRestaurantRatingAsync(id, command);
    }

    public Task<bool> AddRestaurantReviewAsync(long id, CreateRestaurantReviewCommand command)
    {
        ValidateScore(command.TasteScore, nameof(command.TasteScore));
        ValidateScore(command.ServiceScore, nameof(command.ServiceScore));
        ValidateScore(command.EnvironmentScore, nameof(command.EnvironmentScore));
        ValidateScore(command.ValueScore, nameof(command.ValueScore));
        ValidateScore(command.RevisitScore, nameof(command.RevisitScore));

        if (string.IsNullOrWhiteSpace(command.Content) || command.Content.Trim().Length < 30)
        {
            throw new ArgumentException("Restaurant review content must be at least 30 characters.", nameof(command.Content));
        }

        if (command.PricePerPerson is < 0)
        {
            throw new ArgumentException("Price per person cannot be negative.", nameof(command.PricePerPerson));
        }

        return repository.AddRestaurantReviewAsync(id, command);
    }

    public Task<IReadOnlyList<RestaurantReviewListItem>> GetRestaurantReviewsAsync(long id, int limit)
    {
        return repository.GetRestaurantReviewsAsync(id, Math.Clamp(limit, 1, 100));
    }

    private static void ValidateScore(decimal score, string parameterName)
    {
        if (score is < 1m or > 5m)
        {
            throw new ArgumentException("Restaurant review score must be between 1 and 5.", parameterName);
        }
    }
}
