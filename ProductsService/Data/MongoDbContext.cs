﻿using MongoDB.Driver;
using ProductsService.Interfaces;
using ProductsService.Models.MongoDbModels;

namespace ProductsService.Data;

public class MongoDbContext : IDisposable
{
    public IMongoCollection<ProductModel> Products { get; set; }

    public MongoDbContext(IAppConfigurationManager configuration)
    {
        var c = new MongoClient(configuration.MongoDefaultConnectionString);
        var db = c.GetDatabase(configuration.MongoDatabaseName);
        Products = db.GetCollection<ProductModel>("products");
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}