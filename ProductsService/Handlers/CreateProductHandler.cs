using MediatR;
using MongoDB.Bson;
using ProductsService.Dtos.Product;
using ProductsService.Interfaces;
using ProductsService.Models.MongoDbModels;

namespace ProductsService.Handlers;

// ReSharper disable once UnusedType.Global
public class CreateProductHandler : IRequestHandler<CreateProductRequest, ObjectId>
{
    private readonly IProductRepository _productRepository;

    public CreateProductHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ObjectId> Handle(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var productId = await _productRepository.CreateProductAsync(request.Product, cancellationToken);
        return productId;
    }
}

public record CreateProductRequest : IRequest<ObjectId>
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