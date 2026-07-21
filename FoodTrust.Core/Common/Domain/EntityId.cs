namespace FoodTrust.Core.Common.Domain;

public readonly record struct EntityId
{
    private EntityId(long value)
    {
        Value = value;
    }

    public long Value { get; }

    public static EntityId Create(long value, string name)
    {
        if (value <= 0)
        {
            throw new ArgumentException($"{name} identifier is required.", name);
        }

        return new EntityId(value);
    }
}
