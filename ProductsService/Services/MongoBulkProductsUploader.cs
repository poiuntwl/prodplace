using MongoDB.Driver;
using ProductsService.Data;
using ProductsService.Interfaces;
using ProductsService.Models.MongoDbModels;

namespace ProductsService.Services;

public class MongoBulkProductsUploader : IBulkProductsUploader
{
    private readonly MongoDbContext _dbContext;

    public MongoBulkProductsUploader(MongoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(long Created, long Updated)> UploadAsync(ICollection<ProductModel> products,
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