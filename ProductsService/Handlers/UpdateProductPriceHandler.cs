using MediatR;
using MongoDB.Bson;
using ProductsService.Interfaces;

namespace ProductsService.Handlers;

// ReSharper disable once UnusedType.Global
public class UpdateProductPriceHandler : IRequestHandler<UpdateProductPriceRequest>
{
    private readonly IProductRepository _productRepository;

    public UpdateProductPriceHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task Handle(UpdateProductPriceRequest priceRequest, CancellationToken cancellationToken)
    {
        await _productRepository.UpdatePriceAsync(priceRequest.Id, priceRequest.Price, cancellationToken);
    }
}

public record UpdateProductPriceRequest(ObjectId Id, decimal Price) : IRequest;