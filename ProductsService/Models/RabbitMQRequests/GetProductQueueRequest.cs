using ProductsService.Dtos.Product;
using ProductsService.Interfaces;

namespace ProductsService.Models.RabbitMQRequests;

public class GetProductQueueRequest : IQueueRequest<ProductDto>
{
    public int Id { get; set; }
}