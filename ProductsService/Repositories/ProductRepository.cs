using MongoDB.Bson;
using MongoDB.Driver;
using ProductsService.Data;
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
}