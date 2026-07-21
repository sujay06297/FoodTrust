using FoodTrust.Core.Restaurants.Models;

namespace FoodTrust.Core.Restaurants.Domain.ValueObjects;

public sealed record ReviewReportStatusName
{
    private ReviewReportStatusName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ReviewReportStatusName Create(string? value)
    {
        var normalized = value?.Trim();
        if (!ReviewReportStatus.IsValid(normalized))
        {
            throw new ArgumentException("Invalid review report status.", nameof(value));
        }

        return new ReviewReportStatusName(normalized!);
    }
}
