using ProductsService.Dtos.Product;
using ProductsService.Models.MongoDbModels;

namespace ProductsService.Interfaces;

public interface IProductService
{
    Task<ProductDto?> GetProductAsync(int id, CancellationToken ct);
    Task<ICollection<ProductDto>> GetProductsAsync(CancellationToken ct);
    Task<int> CreateProductAsync(ProductModel product, CancellationToken ct);
    Task<(long Created, long Updated)> CreateOrUpdateProductsAsync(ICollection<ProductModel> products,
        CancellationToken ct);
}