using Microsoft.EntityFrameworkCore;
using ProductsService.Models.DatabaseModels;
using ProductsService.Models.MongoDbModels;

namespace ProductsService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<ProductModel> Products { get; set; }
    public DbSet<PurchaseModel> Purchases { get; set; }
    public DbSet<CustomerModel> Customers { get; set; }
}