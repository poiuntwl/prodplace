using MediatR;
using ProductsService.Dtos.Product;
using ProductsService.Interfaces;

namespace ProductsService.Handlers;

// ReSharper disable once UnusedType.Global
public class GetProductsHandler : IRequestHandler<GetProductsRequest, ICollection<ProductDto>>
{
    private readonly IProductService _productService;

    public GetProductsHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<ICollection<ProductDto>> Handle(GetProductsRequest request, CancellationToken cancellationToken)
    {
        return await _productService.GetProductsAsync(cancellationToken);
    }
}

public record GetProductsRequest : IRequest<ICollection<ProductDto>>;