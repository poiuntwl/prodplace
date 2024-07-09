using ProductsService.Dtos.Product;

namespace ProductsService.Interfaces;

public interface IProductService
{
    Task<ProductDto?> GetProductAsync(int id, CancellationToken ct);
    Task<ICollection<ProductDto?>> GetProductsAsync(CancellationToken ct);
}