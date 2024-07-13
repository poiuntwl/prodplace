using ProductsService.Dtos.Product;
using ProductsService.Interfaces;
using ProductsService.Models.MongoDbModels;

namespace ProductsService.Models.RabbitMQRequests;

public class UpdateProductQueueRequest : IQueueRequest
{
    public UpdateProductQueueRequest(UpdateProductRequestDto product)
    {
        Product = new ProductModel
        {
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            CustomFields = product.CustomFields,
        };
    }

    public ProductModel Product { get; set; }
}