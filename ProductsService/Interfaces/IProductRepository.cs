using ProductsService.Models;

namespace ProductsService.Interfaces;

public interface IProductRepository
{
    Task<ProductModel?> GetProduct(int id, CancellationToken ct);
}