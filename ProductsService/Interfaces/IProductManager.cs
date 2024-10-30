using MongoDB.Bson;
using ProductsService.Dtos.Product;
using ProductsService.Models.MongoDbModels;

namespace ProductsService.Interfaces;

public interface IProductManager
{
    Task<ProductDto?> GetProductAsync(ObjectId id, CancellationToken ct);
    Task<ICollection<ProductDto>> GetProductsAsync(CancellationToken ct);
    Task<ObjectId> CreateProductAsync(ProductModel product, CancellationToken cancellationToken);
    Task UpdatePriceAsync(ObjectId productId, decimal price, CancellationToken cancellationToken);
}