using MongoDB.Driver;

namespace ProdPlaceMongoDatabaseTools;

public abstract class MongoDbContextBase : IDisposable
{
    protected readonly MongoClient MongoClient;
    protected readonly IMongoDatabase MongoDatabase;

    public MongoDbContextBase(MongoDbContextConfiguration configuration)
    {
        MongoClient = new MongoClient(configuration.ConnectionString);
        MongoDatabase = MongoClient.GetDatabase(configuration.DatabaseName);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}