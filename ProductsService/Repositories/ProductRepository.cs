using MongoDB.Driver;
using MongoDB.Driver.Linq;
using ProductsService.Data;
using ProductsService.Interfaces;
using ProductsService.Models;

namespace ProductsService.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly MongoDbContext _dbContext;

    public ProductRepository(MongoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProductModel?> GetProductAsync(int id, CancellationToken ct)
    {
        var product = await _dbContext.Products.AsQueryable().FirstOrDefaultAsync(x => x.Id == id, ct);
        return product;
    }

    public async Task<IEnumerable<ProductModel>> GetProductsAsync(CancellationToken ct)
    {
        var products = await _dbContext.Products.AsQueryable().ToListAsync(ct);
        return products;
    }

    public async Task<int> CreateProductAsync(ProductModel product, CancellationToken ct)
    {
        await _dbContext.Products.InsertOneAsync(product, new InsertOneOptions(), ct);
        return product.Id;
    }
}