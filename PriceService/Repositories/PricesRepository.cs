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
        var filter = Builders<PriceModel>.Filter.Eq(p => p.ProductId, productId);
        var currentDoc = await _dbContext.Prices
            .Find(filter)
            .Project(p => new { p.Amount, p.UpdatedAt })
            .FirstOrDefaultAsync(ct);

        // Skip update if amount hasn't changed
        if (currentDoc != null && currentDoc.Amount == priceAmount)
            return true;

        var update = Builders<PriceModel>.Update
            .Set(x => x.Amount, priceAmount)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);
        
        var options = new UpdateOptions { IsUpsert = createIfNotExists };

        try
        {
            var result = await _dbContext.Prices.UpdateOneAsync(
                filter,
                update,
                options,
                ct);

            return result.IsAcknowledged && 
                   (result.ModifiedCount > 0 || 
                    (createIfNotExists && result.UpsertedId != null));
        }
        catch (MongoException)
        {
            return false;
        }
    }

    public async Task<bool> UpdatePriceOldAsync(int productId, decimal priceAmount, CancellationToken ct,
        bool createIfNotExists = false)
    {
        var filter = Builders<ProductModel>.Filter.Eq(p => p.Id, productId);
        var update = Builders<ProductModel>.Update.Set(x => x.Price, priceAmount);
        var options = new UpdateOptions { IsUpsert = createIfNotExists };

        try
        {
            // Check existing value first
            var currentPrice = await _dbContext.Products
                .Find(filter)
                .Project(p => p.Price)
                .FirstOrDefaultAsync(ct);

            if (currentPrice == priceAmount)
                return true;

            var result = await _dbContext.Products.UpdateOneAsync(
                filter, 
                update, 
                options, 
                ct);

            return result.IsAcknowledged && 
                   (result.ModifiedCount > 0 || 
                    result.MatchedCount > 0 || 
                    result.UpsertedId != null);
        }
        catch (MongoException)
        {
            return false;
        }
    }
}