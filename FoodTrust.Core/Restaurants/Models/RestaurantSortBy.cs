namespace FoodTrust.Core.Restaurants.Models;

public static class RestaurantSortBy
{
    public const string Latest = "latest";
    public const string Ranking = "ranking";
    public const string ReviewCount = "reviewCount";

    public static bool IsValid(string? sortBy)
    {
        return string.IsNullOrWhiteSpace(sortBy) ||
            sortBy is Latest or Ranking or ReviewCount;
    }
}
