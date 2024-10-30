using MediatR;
using MongoDB.Bson;
using ProductsService.Interfaces;

namespace ProductsService.Handlers;

// ReSharper disable once UnusedType.Global
public class UpdateProductPriceHandler : IRequestHandler<UpdateProductPriceRequest>
{
    private readonly IProductManager _productManager;

    public UpdateProductPriceHandler(IProductManager productManager)
    {
        _productManager = productManager;
    }

    public async Task Handle(UpdateProductPriceRequest priceRequest, CancellationToken cancellationToken)
    {
        await _productManager.UpdatePriceAsync(priceRequest.Id, priceRequest.Price, cancellationToken);

    }
}

public record UpdateProductPriceRequest(ObjectId Id, decimal Price) : IRequest;