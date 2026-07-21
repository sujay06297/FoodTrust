using FoodTrust.Core.Restaurants.Domain.ValueObjects;
using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Core.Restaurants.Domain;

public sealed class Restaurant
{
    private Restaurant(
        long id,
        RestaurantName name,
        RestaurantAddress address,
        string? phoneNumber,
        string? branchName,
        string? city,
        string? district,
        decimal? latitude,
        decimal? longitude,
        string? openingHours,
        PriceRange priceRange,
        string? cuisineType,
        string? tags,
        string? description,
        string? officialUrl,
        string? googleMapUrl,
        string status)
    {
        Id = id;
        Name = name;
        Address = address;
        PhoneNumber = NormalizeOptional(phoneNumber);
        BranchName = NormalizeOptional(branchName);
        City = NormalizeOptional(city);
        District = NormalizeOptional(district);
        Latitude = latitude;
        Longitude = longitude;
        OpeningHours = NormalizeOptional(openingHours);
        PriceRange = priceRange;
        CuisineType = NormalizeOptional(cuisineType);
        Tags = NormalizeOptional(tags);
        Description = NormalizeOptional(description);
        OfficialUrl = NormalizeOptional(officialUrl);
        GoogleMapUrl = NormalizeOptional(googleMapUrl);
        Status = status;
    }

    public long Id { get; }

    public RestaurantName Name { get; private set; }

    public RestaurantAddress Address { get; private set; }

    public string? PhoneNumber { get; private set; }

    public string? BranchName { get; private set; }

    public string? City { get; private set; }

    public string? District { get; private set; }

    public decimal? Latitude { get; private set; }

    public decimal? Longitude { get; private set; }

    public string? OpeningHours { get; private set; }

    public PriceRange PriceRange { get; private set; }

    public string? CuisineType { get; private set; }

    public string? Tags { get; private set; }

    public string? Description { get; private set; }

    public string? OfficialUrl { get; private set; }

    public string? GoogleMapUrl { get; private set; }

    public string Status { get; private set; }

    public static Restaurant Create(CreateRestaurantCommand command)
    {
        return new Restaurant(
            0,
            RestaurantName.Create(command.Name),
            RestaurantAddress.Create(command.Address),
            command.PhoneNumber,
            command.BranchName,
            command.City,
            command.District,
            command.Latitude,
            command.Longitude,
            command.OpeningHours,
            PriceRange.Create(command.PriceMin, command.PriceMax),
            command.CuisineType,
            command.Tags,
            command.Description,
            command.OfficialUrl,
            command.GoogleMapUrl,
            RestaurantStatus.PendingReview);
    }

    public static Restaurant Restore(long id, UpdateRestaurantCommand command, string status)
    {
        EnsureValidStatus(status);

        return new Restaurant(
            id,
            RestaurantName.Create(command.Name),
            RestaurantAddress.Create(command.Address),
            command.PhoneNumber,
            command.BranchName,
            command.City,
            command.District,
            command.Latitude,
            command.Longitude,
            command.OpeningHours,
            PriceRange.Create(command.PriceMin, command.PriceMax),
            command.CuisineType,
            command.Tags,
            command.Description,
            command.OfficialUrl,
            command.GoogleMapUrl,
            status);
    }

    public void ChangeStatus(string status)
    {
        EnsureValidStatus(status);
        Status = status.Trim();
    }

    public CreateRestaurantCommand ToCreateCommand()
    {
        return new CreateRestaurantCommand(
            Name.Value,
            Address.Value,
            PhoneNumber,
            BranchName,
            City,
            District,
            Latitude,
            Longitude,
            OpeningHours,
            PriceRange.Minimum,
            PriceRange.Maximum,
            CuisineType,
            Tags,
            Description,
            OfficialUrl,
            GoogleMapUrl);
    }

    public UpdateRestaurantCommand ToUpdateCommand()
    {
        return new UpdateRestaurantCommand(
            Name.Value,
            Address.Value,
            PhoneNumber,
            BranchName,
            City,
            District,
            Latitude,
            Longitude,
            OpeningHours,
            PriceRange.Minimum,
            PriceRange.Maximum,
            CuisineType,
            Tags,
            Description,
            OfficialUrl,
            GoogleMapUrl);
    }

    private static void EnsureValidStatus(string? status)
    {
        if (!RestaurantStatus.IsValid(status?.Trim()))
        {
            throw new ArgumentException("Invalid restaurant status.", nameof(status));
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
