using FoodTrust.Core.RestaurantImports.Models;

namespace FoodTrust.Core.RestaurantImports.Domain.ValueObjects;

public sealed record CandidateRestaurantLifecycleStatus
{
    private CandidateRestaurantLifecycleStatus(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public bool IsApproved => Value == CandidateRestaurantStatus.Approved;

    public bool IsRejected => Value == CandidateRestaurantStatus.Rejected;

    public static CandidateRestaurantLifecycleStatus Create(string? value)
    {
        var normalized = value?.Trim();
        if (!CandidateRestaurantStatus.IsValid(normalized))
        {
            throw new ArgumentException("Invalid candidate restaurant status.", nameof(value));
        }

        return new CandidateRestaurantLifecycleStatus(normalized!);
    }

    public override string ToString()
    {
        return Value;
    }
}
