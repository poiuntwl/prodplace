using MongoDB.Bson;
using ProductsService.Models.MongoDbModels;

namespace ProductsService.Interfaces;

public interface IProductRepository
{
    Task<ProductModel?> GetProductAsync(ObjectId id, CancellationToken ct);
    Task<IEnumerable<ProductModel>> GetProductsAsync(CancellationToken ct);
    Task<ObjectId> CreateProductAsync(ProductModel product, CancellationToken ct);
}