using ProductsService.Dtos.Product;
using ProductsService.Models;

namespace ProductsService.Interfaces;

public interface IProductService
{
    Task<ProductDto?> GetProductAsync(int id, CancellationToken ct);
    Task<ICollection<ProductDto?>> GetProductsAsync(CancellationToken ct);
    Task<int> CreateProductAsync(ProductModel product, CancellationToken ct);
}