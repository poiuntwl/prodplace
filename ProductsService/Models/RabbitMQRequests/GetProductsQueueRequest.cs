using ProductsService.Dtos.Product;
using ProductsService.Interfaces;

namespace ProductsService.Models.RabbitMQRequests;

public class GetProductsQueueRequest : IQueueRequest<ICollection<ProductDto>>;