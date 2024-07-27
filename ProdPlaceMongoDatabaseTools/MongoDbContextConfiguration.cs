namespace ProdPlaceDatabaseTools;

public class MongoDbContextConfiguration
{
    public required string ConnectionString { get; init; }
    public required string DatabaseName { get; init; }
}