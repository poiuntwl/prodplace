using Microsoft.EntityFrameworkCore;
using ProductsService.Data;
using ProductsService.Helpers;
using ProductsService.Interfaces;
using ProductsService.Repositories;
using ProductsService.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceInjectionExtensions
{
    public static IServiceCollection AddDbServices(this IServiceCollection s, WebApplicationBuilder builder) =>
        s
            .AddSingleton<IAppConfigurationManager, AppConfigurationManager>()
            .AddDbContext<AppDbContext>(o =>
            {
                o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            })
            .AddSingleton<MongoDbContext>();

    public static IServiceCollection AddProductServices(this IServiceCollection s) =>
        s
            .AddHostedService<ProductRpcConsumer>()
            .AddScoped<IProductService, ProductService>()
            .AddScoped<IProductRepository, ProductRepository>()
            .AddScoped<IProductRequestRouter, ProductProductRequestRouter>()
            .AddTransient<IAuthService, AuthService>();
}