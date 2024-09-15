using ProductsService.Dtos.Product;
using ProductsService.Interfaces;
using ProductsService.Mappers;
using ProductsService.Models.MongoDbModels;
using ProductsService.Validators;

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

    public async Task<ICollection<ProductDto>> GetProductsAsync(CancellationToken ct)
    {
        var products = await _productRepository.GetProductsAsync(ct);
        return products.Select<ProductModel, ProductDto>(x => x.ToDto()).ToList();
    }

    public async Task<int> CreateProductAsync(ProductModel product, CancellationToken ct)
    {
        var validator = new CreateProductValidator();
        if ((await validator.ValidateAsync(product, ct)).IsValid == false)
        {
            return -1;
        }

        var productId = await _productRepository.CreateProductAsync(product, ct);
        return productId;
    }

    public async Task<(long Created, long Updated)> CreateOrUpdateProductsAsync(ICollection<ProductModel> products,
        CancellationToken ct)
    {
        var result = await _productRepository.CreateOrUpdateProductsAsync(products, ct);
        return result;
    }
}