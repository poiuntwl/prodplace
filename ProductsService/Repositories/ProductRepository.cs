using Microsoft.EntityFrameworkCore;
using ProductsService.Data;
using ProductsService.Interfaces;
using ProductsService.Models;

namespace ProductsService.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _dbContext;

    public ProductRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProductModel?> GetProduct(int id, CancellationToken ct)
    {
        var product = await _dbContext.Products.FindAsync([id], cancellationToken: ct);
        return product;
    }

    public async Task<IEnumerable<ProductModel>> GetProducts(CancellationToken ct)
    {
        var products = await _dbContext.Products.ToListAsync(cancellationToken: ct);
        return products;
    }
}