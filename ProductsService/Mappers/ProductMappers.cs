using ProductsService.Dtos.Product;
using ProductsService.Models;

namespace ProductsService.Mappers;

public static class ProductMappers
{
    public static ProductDto? ToDto(this ProductModel model)
    {
        return new ProductDto
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            CustomFields = model.CustomFields
        };
    }
}