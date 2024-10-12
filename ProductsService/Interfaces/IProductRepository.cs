using MongoDB.Bson;
using ProductsService.Dtos.Product;
using ProductsService.Models.MongoDbModels;

namespace ProductsService.Interfaces;

public interface IProductRepository
{
    Task<ProductModel?> GetProductAsync(ObjectId id, CancellationToken ct);
    Task<IEnumerable<ProductModel>> GetProductsAsync(CancellationToken ct);
    Task<ObjectId> CreateProductAsync(ProductModel product, CancellationToken ct);
    Task<bool> UpdatePriceAsync(ObjectId id, decimal price, CancellationToken ct);
}