using ProductsService.Dtos.Product;
using ProductsService.Interfaces;
using ProductsService.Mappers;

namespace ProductsService.Models.RabbitMQRequests;

public class BulkCreateUpdateQueueRequest : IQueueRequest<BulkCreateUpdateQueueResponse>
{
    public BulkCreateUpdateQueueRequest()
    {
    }

    public BulkCreateUpdateQueueRequest(ICollection<ProductDto> products)
    {
        Products = products.Select(x => x.ToModel()).ToList();
    }

    public ICollection<ProductModel> Products { get; set; } = new List<ProductModel>();
}