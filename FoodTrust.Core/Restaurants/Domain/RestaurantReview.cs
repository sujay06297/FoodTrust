using FoodTrust.Core.Common.Domain;
using FoodTrust.Core.Restaurants.Domain.ValueObjects;
using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Core.Restaurants.Domain;

public sealed class RestaurantReview
{
    private const int RepeatReviewWindowDays = 30;

    private RestaurantReview(long restaurantId, CreateRestaurantReviewCommand command)
    {
        RestaurantId = EntityId.Create(restaurantId, nameof(restaurantId));
        UserId = EntityId.Create(command.UserId, nameof(command.UserId));
        TasteScore = ReviewScore.Create(command.TasteScore, nameof(command.TasteScore));
        ServiceScore = ReviewScore.Create(command.ServiceScore, nameof(command.ServiceScore));
        EnvironmentScore = ReviewScore.Create(command.EnvironmentScore, nameof(command.EnvironmentScore));
        ValueScore = ReviewScore.Create(command.ValueScore, nameof(command.ValueScore));
        RevisitScore = ReviewScore.Create(command.RevisitScore, nameof(command.RevisitScore));
        Content = ReviewContent.Create(command.Content);
        PricePerPerson = PricePerPerson.Create(command.PricePerPerson);
    }

    public EntityId RestaurantId { get; }

    public EntityId UserId { get; }

    public ReviewScore TasteScore { get; }

    public ReviewScore ServiceScore { get; }

    public ReviewScore EnvironmentScore { get; }

    public ReviewScore ValueScore { get; }

    public ReviewScore RevisitScore { get; }

    public ReviewContent Content { get; }

    public PricePerPerson PricePerPerson { get; }

    public static RestaurantReview Create(long restaurantId, CreateRestaurantReviewCommand command)
    {
        return new RestaurantReview(restaurantId, command);
    }

    public static DateTime RepeatReviewWindowStart(DateTimeOffset now)
    {
        return now.AddDays(-RepeatReviewWindowDays).UtcDateTime;
    }
}
