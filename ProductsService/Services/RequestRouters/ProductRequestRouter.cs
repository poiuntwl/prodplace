using System;
using System.Threading;
using System.Threading.Tasks;
using ProductsService.Interfaces;
using ProductsService.Models;
using ProductsService.Models.RabbitMQRequests;

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
            GetProductQueueRequest req => await _productService.GetProductAsync(req.Id, ct),
            GetProductsQueueRequest => await _productService.GetProductsAsync(ct),
            CreateProductQueueRequest req => await _productService.CreateProductAsync(req.Product, ct),
            BulkCreateUpdateQueueRequest req => await _productService.CreateOrUpdateProductsAsync(req.Products, ct),
            _ => throw new ArgumentException($"Unsupported request type: {request.GetType().Name}")
        };
    }
}