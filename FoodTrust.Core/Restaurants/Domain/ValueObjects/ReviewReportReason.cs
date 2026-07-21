namespace FoodTrust.Core.Restaurants.Domain.ValueObjects;

public sealed record ReviewReportReason
{
    private ReviewReportReason(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ReviewReportReason Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Review report reason type is required.", nameof(value));
        }

        return new ReviewReportReason(value.Trim());
    }
}
