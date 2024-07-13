using ProductsService.Dtos.Product;
using ProductsService.Models.MongoDbModels;

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

    public static ProductModel ToModel(this ProductDto model)
    {
        return new ProductModel
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            CustomFields = model.CustomFields
        };
    }
}