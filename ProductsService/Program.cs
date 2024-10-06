using ProductsService.Interfaces;
using ProductsService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
var s = builder.Services;
// s.AddJwtAuthConfiguration(builder.Configuration);
s.AddControllers();
s.AddEndpointsApiExplorer();
s.AddSwaggerGen();
s.AddDbServices(builder);
s.AddHealthChecks();

s.AddSingleton<IRabbitMqRpcClient, RabbitMqRpcClient>();
s.AddProductServices();

var app = builder.Build();
// app.UseJwtAuthConfiguration();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapHealthChecks("/api/health");

app.Run();