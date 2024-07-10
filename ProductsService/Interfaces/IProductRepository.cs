using ProductsService.Models;

namespace ProductsService.Interfaces;

public interface IProductRepository
{
    Task<ProductModel?> GetProductAsync(int id, CancellationToken ct);
    Task<IEnumerable<ProductModel>> GetProductsAsync(CancellationToken ct);
    Task<int> CreateProductAsync(ProductModel product, CancellationToken ct);
    Task<(long Created, long Updated)> CreateOrUpdateProductsAsync(ICollection<ProductModel> products,
        CancellationToken ct);
}