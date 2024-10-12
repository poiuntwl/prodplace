using MongoDB.Bson;

namespace ProductsService.Dtos.Product;

public class UpdateProductPriceRequestDto
{
    public string Id { get; set; }
    public decimal Price { get; set; }
}