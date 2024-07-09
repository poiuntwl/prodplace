using ProductsService.Data;
using ProductsService.Dtos.Product;
using ProductsService.Interfaces;
using ProductsService.Mappers;
using ProductsService.Models;

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
        var product = await _productRepository.GetProductAsync(id, ct);
        return product?.ToDto();
    }

    public async Task<ICollection<ProductDto?>> GetProductsAsync(CancellationToken ct)
    {
        var products = await _productRepository.GetProductsAsync(ct);
        return products.Select(x => x.ToDto()).ToList();
    }

    public async Task<int> CreateProductAsync(ProductModel product, CancellationToken ct)
    {
        var productId = await _productRepository.CreateProductAsync(product, ct);
        return productId;
    }
}