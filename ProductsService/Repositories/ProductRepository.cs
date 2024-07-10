using MongoDB.Bson;
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
        await _dbContext.Products.InsertOneAsync(product, cancellationToken: ct);
        return product.Id;
    }

    public async Task<(long Created, long Updated)> CreateOrUpdateProductsAsync(ICollection<ProductModel> products,
        CancellationToken ct)
    {
        var bulkOps = new List<WriteModel<ProductModel>>();

        foreach (var product in products)
        {
            var filter = Builders<ProductModel>.Filter.Eq(x => x.Id, product.Id);

            var update = Builders<ProductModel>.Update
                .Set("Name", product.Name)
                .Set("Description", product.Description)
                .Set("Price", product.Price)
                .Set("CustomFields", product.CustomFields)
                .SetOnInsert("CreatedAt", DateTime.UtcNow);

            var upsert = new UpdateOneModel<ProductModel>(filter, update) { IsUpsert = true };
            bulkOps.Add(upsert);
        }

        var result = await _dbContext.Products.BulkWriteAsync(bulkOps, cancellationToken: ct);

        return (result.InsertedCount, result.ModifiedCount);
    }
}