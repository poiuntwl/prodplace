using MongoDB.Driver;
using PriceService.Models;
using ProdPlaceDatabaseTools;

namespace PriceService.Db;

public class MongoDbContext : MongoDbContextBase
{
    public IMongoCollection<PriceModel> Prices { get; set; }
    public IMongoCollection<ProductModel> Products { get; set; }

    public MongoDbContext(MongoDbContextConfiguration configuration) : base(configuration)
    {
        Prices = MongoDatabase.GetCollection<PriceModel>("prices");
        Products = MongoDatabase.GetCollection<ProductModel>("products");
    }
}