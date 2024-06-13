using Bogus;
using ProdPlace.Models;

namespace ProdPlace.db;

public static class DbInitializer
{
    public static void Initialize(ProductsDbContext ctx)
    {
        ctx.Database.EnsureCreated();

        // Check if the database is already seeded
        if (ctx.Products.Any())
        {
            return; // Database has already been seeded
        }

        // Seed the database with initial data
        var productFaker = new Faker<ProductModel>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
            .RuleFor(p => p.Price, f => f.Random.Double(1, 1000));

        // Generate random product data
        var products = productFaker.Generate(10); // Generate 10 random products

        // Add the generated products to the database
        ctx.Products.AddRange(products);
        ctx.SaveChanges();
    }
}