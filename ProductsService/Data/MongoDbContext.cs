using MongoDB.Driver;
using ProdPlaceDatabaseTools;
using ProductsService.Interfaces;
using ProductsService.Models.MongoDbModels;

namespace ProductsService.Data;

public class MongoDbContext : MongoDbContextBase
{
    public IMongoCollection<ProductModel> Products { get; set; }

    public MongoDbContext(IAppConfigurationManager configuration) : base(new MongoDbContextConfiguration
    {
        ConnectionString = configuration.MongoDefaultConnectionString ?? string.Empty,
        DatabaseName = configuration.MongoDatabaseName ?? string.Empty
    })
    {
        Products = MongoDatabase.GetCollection<ProductModel>("products");
    }
}