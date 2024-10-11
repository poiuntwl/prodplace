using Microsoft.EntityFrameworkCore;
using ProductsService.Data;
using ProductsService.Helpers;
using ProductsService.Interfaces;
using ProductsService.Middleware;
using ProductsService.Repositories;
using ProductsService.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceInjectionExtensions
{
    public static IServiceCollection AddDbServices(this IServiceCollection s, WebApplicationBuilder builder)
    {
        s.AddSingleton<IAppConfigurationManager, AppConfigurationManager>();
        s.AddDbContext<AppDbContext>(o =>
        {
            o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });
        s.AddSingleton<MongoDbContext>();
        return s;
    }

    public static IServiceCollection AddProductServices(this IServiceCollection s)
    {
        s.AddScoped<IProductService, ProductService>();
        s.AddScoped<IProductRepository, ProductRepository>();
        s.AddScoped<IAuthService, AuthService>();
        s.AddTransient<IBulkProductsUploader, MongoBulkProductsUploader>(); // potentially move to S3

        return s;
    }

    public static IServiceCollection AddValidationMiddleware(this IServiceCollection s)
    {
        s.AddTransient<TokenValidationMiddleware>();
        s.AddTransient<RoleValidationMiddleware>();

        return s;
    }

    public static WebApplication UseValidationMiddleware(this WebApplication app)
    {
        app.UseMiddleware<TokenValidationMiddleware>();
        app.UseMiddleware<RoleValidationMiddleware>();

        return app;
    }
}