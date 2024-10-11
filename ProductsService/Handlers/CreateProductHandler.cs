using MediatR;
using ProductsService.Dtos.Product;
using ProductsService.Interfaces;
using ProductsService.Models.MongoDbModels;

namespace ProductsService.Handlers;

// ReSharper disable once UnusedType.Global
public class CreateProductHandler : IRequestHandler<CreateProductRequest, int>
{
    private readonly IProductService _productService;

    public CreateProductHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<int> Handle(CreateProductRequest request, CancellationToken cancellationToken)
    {
        return await _productService.CreateProductAsync(request.Product, cancellationToken);
    }
}

public record CreateProductRequest : IRequest<int>
{
    public CreateProductRequest(CreateProductRequestDto dto)
    {
        Product = new ProductModel
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            CustomFields = dto.CustomFields
        };
    }

    public ProductModel Product { get; init; }
}