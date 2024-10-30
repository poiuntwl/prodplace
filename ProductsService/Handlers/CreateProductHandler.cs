using MediatR;
using MongoDB.Bson;
using ProductsService.Dtos.Product;
using ProductsService.Interfaces;
using ProductsService.Models.MongoDbModels;

namespace ProductsService.Handlers;

// ReSharper disable once UnusedType.Global
public class CreateProductHandler : IRequestHandler<CreateProductRequest, ObjectId>
{
    private readonly IProductManager _productManager;

    public CreateProductHandler(IProductManager productManager)
    {
        _productManager = productManager;
    }

    public async Task<ObjectId> Handle(CreateProductRequest request, CancellationToken cancellationToken)
    {
        return await _productManager.CreateProductAsync(request.Product, cancellationToken);
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