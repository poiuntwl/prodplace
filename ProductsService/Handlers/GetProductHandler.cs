using MediatR;
using MongoDB.Bson;
using ProductsService.Dtos.Product;
using ProductsService.Interfaces;

namespace ProductsService.Handlers;

// ReSharper disable once UnusedType.Global
public class GetProductHandler : IRequestHandler<GetProductRequest, ProductDto?>
{
    private readonly IProductManager _productManager;

    public GetProductHandler(IProductManager productManager)
    {
        _productManager = productManager;
    }

    public async Task<ProductDto?> Handle(GetProductRequest request, CancellationToken cancellationToken)
    {
        return await _productManager.GetProductAsync(request.Id, cancellationToken);
    }
}

public record GetProductRequest(ObjectId Id) : IRequest<ProductDto?>;