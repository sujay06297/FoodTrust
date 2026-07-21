namespace FoodTrust.Core.RestaurantImports.Domain;

public readonly record struct ImportBatchSize
{
    private ImportBatchSize(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static ImportBatchSize Create(int value)
    {
        return new ImportBatchSize(Math.Max(1, value));
    }
}
