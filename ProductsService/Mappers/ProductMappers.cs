using System.Text.Json;
using ProductsService.Dtos.Product;
using ProductsService.Models.MongoDbModels;

namespace ProductsService.Mappers;

public static class ProductMappers
{
    public static ProductDto ToDto(this ProductModel model)
    {
        var fieldsDeserialized = JsonSerializer.Deserialize<Dictionary<string, string>>(model.CustomFields) ?? new Dictionary<string, string>();
        return new ProductDto
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            CustomFields = fieldsDeserialized
        };
    }

    public static ProductModel ToModel(this ProductDto model)
    {
        var fieldsSerialized = JsonSerializer.Serialize(model.CustomFields);
        return new ProductModel
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            CustomFields = fieldsSerialized
        };
    }
}