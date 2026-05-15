namespace FoodTrust.Infrastructure.Data;

public sealed record DatabaseMigration(
    long Version,
    string Name,
    string Sql);
