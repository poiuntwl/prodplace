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
s.AddHttpClient<AuthHttpClient>((_, client) =>
{
    client.BaseAddress = new Uri(builder.Configuration["Auth:IdentityServiceConnectionString"]!);
});
s.AddScoped<IAuthService, AuthService>();

s.AddSingleton<IRabbitMqRpcClient, RabbitMqRpcClient>();
s.AddProductServices();

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
app.UseMiddleware<TokenValidationMiddleware>();
app.UseMiddleware<RoleValidationMiddleware>();

app.Run();