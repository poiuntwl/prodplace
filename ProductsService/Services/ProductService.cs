﻿using MongoDB.Bson;
using ProductsService.Dtos.Product;
using ProductsService.Interfaces;
using ProductsService.Mappers;
using ProductsService.Models.MongoDbModels;
using ProductsService.Validators;

namespace ProductsService.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDto?> GetProductAsync(ObjectId id, CancellationToken ct)
    {
        var product = await _productRepository.GetProductAsync(id, ct);
        return product?.ToDto();
    }

    public async Task<ICollection<ProductDto>> GetProductsAsync(CancellationToken ct)
    {
        var products = await _productRepository.GetProductsAsync(ct);
        return products.Select<ProductModel, ProductDto>(x => x.ToDto()).ToList();
    }
}