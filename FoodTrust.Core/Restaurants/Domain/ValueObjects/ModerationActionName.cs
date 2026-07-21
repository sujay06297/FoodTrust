using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Core.Restaurants.Domain.ValueObjects;

public sealed record ModerationActionName
{
    private ModerationActionName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ModerationActionName? CreateOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (!ReviewModerationAction.IsValid(normalized))
        {
            throw new ArgumentException("Invalid review moderation action.", nameof(value));
        }

        return new ModerationActionName(normalized);
    }
}
