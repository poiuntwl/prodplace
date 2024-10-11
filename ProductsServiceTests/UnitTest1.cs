using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductsService;
using ProductsService.Data;
using ProductsService.Helpers;
using ProductsService.Interfaces;
using ProductsService.Repositories;
using ProductsService.Services;

namespace ProductsServiceTests;

public class UnitTest1
{
    private readonly IServiceProvider _serviceProvider;

    public UnitTest1()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IAppConfigurationManager, AppConfigurationManager>();

        services.AddDbContext<AppDbContext>(o =>
        {
            o.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddSingleton<MongoDbContext>();

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductRepository, ProductRepository>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task Test1()
    {
        var productService = _serviceProvider.GetRequiredService<IProductService>();
        var r = await productService.GetProductsAsync(CancellationToken.None);
    }
}