﻿using ProductsService.Dtos.Product;
using ProductsService.Interfaces;
using ProductsService.Models.MongoDbModels;

namespace ProductsService.Models.RabbitMQRequests;

public class CreateProductQueueRequest : IQueueRequest<int>
{
    public CreateProductQueueRequest(CreateProductRequestDto product)
    {
        Product = new ProductModel
        {
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            CustomFields = product.CustomFields
        };
    }

    public ProductModel Product { get; set; }
}