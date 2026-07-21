using FoodTrust.Core.Common.Domain;

namespace FoodTrust.Core.Restaurants.Domain;

public sealed record FavoriteRestaurant
{
    private FavoriteRestaurant(EntityId userId, EntityId restaurantId)
    {
        UserId = userId;
        RestaurantId = restaurantId;
    }

    public EntityId UserId { get; }

    public EntityId RestaurantId { get; }

    public static FavoriteRestaurant Create(long userId, long restaurantId)
    {
        return new FavoriteRestaurant(
            EntityId.Create(userId, nameof(userId)),
            EntityId.Create(restaurantId, nameof(restaurantId)));
    }
}
