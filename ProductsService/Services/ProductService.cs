using ProductsService.Data;
using ProductsService.Dtos.Product;
using ProductsService.Interfaces;
using ProductsService.Mappers;

namespace ProductsService.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDto?> GetProductAsync(int id, CancellationToken ct)
    {
        var product = await _productRepository.GetProduct(id, ct);
        return product?.ToDto();
    }
}