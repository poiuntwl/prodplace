using AuthConfiguration;
using ProductsService.Interfaces;
using ProductsService.Middleware;
using ProductsService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
var s = builder.Services;
s.AddControllers();
s.AddEndpointsApiExplorer();
s.AddSwaggerGen();
s.AddDbServices(builder);
s.AddHealthChecks();
s.AddJwtAuthConfiguration(builder.Configuration);
s.AddGrpcClient<IdentityGrpc.Server.IdentityService.IdentityServiceClient>(x =>
{
    x.Address = new Uri("https://localhost:44304");
});

s.AddProductServices();
s.AddValidationMiddleware();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapHealthChecks("/api/health");
app.UseJwtAuthConfiguration();
app.UseValidationMiddleware();

app.Run();