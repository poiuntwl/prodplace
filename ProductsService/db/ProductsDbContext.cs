using Microsoft.EntityFrameworkCore;
using ProdPlace.Models;

namespace ProdPlace.db;

public class ProductsDbContext : DbContext
{
    public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options)
    {

    }

    public DbSet<ProductModel> Products { get; set; }
}