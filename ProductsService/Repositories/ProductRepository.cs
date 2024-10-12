using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using ProductsService.Data;
using ProductsService.Dtos.Product;
using ProductsService.Interfaces;
using ProductsService.Models.MongoDbModels;

namespace ProductsService.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly MongoDbContext _dbContext;

    public ProductRepository(MongoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProductModel?> GetProductAsync(ObjectId id, CancellationToken ct)
    {
        var product = await _dbContext.Products.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken: ct);
        return product;
    }

    public async Task<IEnumerable<ProductModel>> GetProductsAsync(CancellationToken ct)
    {
        var products = await _dbContext.Products.AsQueryable().ToListAsync(ct);
        return products;
    }

    public async Task<ObjectId> CreateProductAsync(ProductModel product, CancellationToken ct)
    {
        product.Id = ObjectId.GenerateNewId();

        await _dbContext.Products.InsertOneAsync(product, cancellationToken: ct);
        return product.Id;
    }

    public async Task<bool> UpdatePriceAsync(ObjectId id, decimal price, CancellationToken ct)
    {
        var filter = Builders<ProductModel>.Filter.Eq(x => x.Id, id);
        var update = Builders<ProductModel>.Update.Set(x => x.Price, price);

        var result = await _dbContext.Products.UpdateOneAsync(filter, update, cancellationToken: ct);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }
}