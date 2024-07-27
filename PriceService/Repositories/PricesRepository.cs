using MongoDB.Driver;
using PriceService.Db;
using PriceService.Interfaces;
using PriceService.Models;

namespace PriceService.Repositories;

public class PricesRepository : IPricesRepository
{
    private readonly MongoDbContext _dbContext;

    public PricesRepository(MongoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> UpdatePriceAsync(int productId, decimal priceAmount, CancellationToken ct,
        bool createIfNotExists = false)
    {
        var filterDef = Builders<PriceModel>.Filter.Eq(p => p.ProductId, productId);
        var updateDef = Builders<PriceModel>.Update
            .Set(x => x.Amount, priceAmount)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);
        var updateOptions = new UpdateOptions
        {
            IsUpsert = createIfNotExists
        };

        try
        {
            var updateResult = await _dbContext.Prices.UpdateOneAsync(
                filterDef,
                updateDef,
                updateOptions,
                ct);

            return updateResult.IsAcknowledged
                   && (updateResult.ModifiedCount > 0 || (createIfNotExists && updateResult.UpsertedId != null));
        }
        catch (MongoException e)
        {
            return false;
        }
    }

    public async Task<bool> UpdatePriceOldAsync(int productId, decimal priceAmount, CancellationToken ct,
        bool createIfNotExists = false)
    {
        var filterDef = Builders<ProductModel>.Filter.Eq(p => p.Id, productId);
        var updateDef = Builders<ProductModel>.Update
            .Set(x => x.Price, priceAmount);
        var updateOptions = new UpdateOptions
        {
            IsUpsert = createIfNotExists
        };

        try
        {
            var updateResult = await _dbContext.Products.UpdateOneAsync(
                filterDef,
                updateDef,
                updateOptions,
                ct);

            if (updateResult.ModifiedCount != 0)
            {
                return true;
            }

            return updateResult is { IsAcknowledged: true, MatchedCount: > 0 };
        }
        catch (MongoException e)
        {
            return false;
        }
    }
}