using ProductsService.Dtos.Product;
using ProductsService.Models;

namespace ProductsService.Mappers;

public static class ProductMappers
{
    public static ProductDto ToDto(this ProductModel model)
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

    public static ProductModel ToModel(this CreateProductRequestDto dto)
    {
        return new ProductModel
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            CustomFields = dto.CustomFields
        };
    }
}