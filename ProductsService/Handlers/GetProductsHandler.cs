using MediatR;
using ProductsService.Dtos.Product;
using ProductsService.Interfaces;

namespace ProductsService.Handlers;

// ReSharper disable once UnusedType.Global
public class GetProductsHandler : IRequestHandler<GetProductsRequest, ICollection<ProductDto>>
{
    private readonly IProductManager _productManager;

    public GetProductsHandler(IProductManager productManager)
    {
        _productManager = productManager;
    }

    public async Task<ICollection<ProductDto>> Handle(GetProductsRequest request, CancellationToken cancellationToken)
    {
        return await _productManager.GetProductsAsync(cancellationToken);
    }
}

public record GetProductsRequest : IRequest<ICollection<ProductDto>>;