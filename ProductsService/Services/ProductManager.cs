using MongoDB.Bson;
using ProductsService.Dtos.Product;
using ProductsService.Interfaces;
using ProductsService.Mappers;
using ProductsService.Models.MongoDbModels;

namespace ProductsService.Services;

public class ProductManager : IProductManager
{
    private readonly IProductRepository _productRepository;

    public ProductManager(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDto?> GetProductAsync(ObjectId id, CancellationToken ct)
    {
        var product = await _productRepository.GetProductAsync(id, ct);
        return product?.ToDto();
    }

    public async Task<ICollection<ProductDto>> GetProductsAsync(CancellationToken ct)
    {
        var products = await _productRepository.GetProductsAsync(ct);
        return products.Select<ProductModel, ProductDto>(x => x.ToDto()).ToList();
    }

    public async Task<ObjectId> CreateProductAsync(ProductModel product, CancellationToken cancellationToken)
    {
        var productId = await _productRepository.CreateProductAsync(product, cancellationToken);
        return productId;
    }

    public async Task UpdatePriceAsync(ObjectId productId, decimal price,
        CancellationToken cancellationToken)
    {
        await _productRepository.UpdatePriceAsync(productId, price, cancellationToken);
    }
}