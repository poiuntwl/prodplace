using System;
using System.Threading;
using System.Threading.Tasks;
using ProductsService.Interfaces;
using ProductsService.Models;

public class ProductProductRequestRouter : IProductRequestRouter
{
    private readonly IProductService _productService;

    public ProductProductRequestRouter(IProductService productService)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
    }

    public async Task<object?> RouteRequestAsync(object request, CancellationToken ct)
    {
        return request switch
        {
            GetProductRequest req => await _productService.GetProductAsync(req.Id, ct),
            GetProductsRequest => await _productService.GetProductsAsync(ct),
            _ => throw new ArgumentException($"Unsupported request type: {request.GetType().Name}")
        };
    }
}