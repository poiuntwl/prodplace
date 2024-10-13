using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using OrderService.Models;

namespace OrderService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<OrderModel> Orders { get; set; }
    public DbSet<OrderItemModel> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderModel>()
            .HasIndex(o => o.CustomerId);

        modelBuilder.Entity<OrderItemModel>()
            .HasIndex(oi => oi.OrderId);

        modelBuilder.Entity<OrderItemModel>()
            .HasIndex(oi => oi.ProductId);

        modelBuilder.Entity<OrderModel>()
            .Property(x => x.Status)
            .HasConversion<string>();

        modelBuilder.Entity<OrderModel>()
            .Property(o => o.OrderDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}