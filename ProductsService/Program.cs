using Microsoft.EntityFrameworkCore;
using ProductsService.Data;
using ProductsService.Interfaces;
using ProductsService.Repositories;
using ProductsService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
var s = builder.Services;
s.AddControllers();
s.AddEndpointsApiExplorer();
s.AddSwaggerGen();
s.AddDbContext<AppDbContext>(o => { o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")); });
s.AddSingleton<IRabbitMQRpcClient, RabbitMQRpcClient>();
s.AddHostedService<ProductRpcConsumer>();
s.AddScoped<IProductService, ProductService>();
s.AddScoped<IProductRepository, ProductRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();