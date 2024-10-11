using MediatR;
using ProductsService.Dtos.Product;
using ProductsService.Interfaces;

namespace ProductsService.Handlers;

// ReSharper disable once UnusedType.Global
public class GetProductHandler : IRequestHandler<GetProductRequest, ProductDto?>
{
    private readonly IProductService _productService;

    public GetProductHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<ProductDto?> Handle(GetProductRequest request, CancellationToken cancellationToken)
    {
        return await _productService.GetProductAsync(request.Id, cancellationToken);
    }
}

public record GetProductRequest(int Id) : IRequest<ProductDto?>;