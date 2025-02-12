using Microsoft.Extensions.Options;
using RoleAdmin;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.Configure<AdminServiceGrpcOptions>(builder.Configuration.GetSection("Grpc"));
builder.Services.AddGrpcClient<RoleAdminService.RoleAdminServiceClient>((serviceProvider, options) =>
{
    var grpcOptions = serviceProvider.GetRequiredService<IOptions<AdminServiceGrpcOptions>>();
    if (string.IsNullOrWhiteSpace(grpcOptions.Value.IdentityServiceConnectionString))
    {
        // Handle the missing URL case!  Throwing an exception is good during startup.
        throw new InvalidOperationException(
            "The gRPC URL is missing from the configuration.  Please set the 'Grpc:Url' value in your appsettings.json or other configuration source.");
    }

    if (!Uri.TryCreate(grpcOptions.Value.IdentityServiceConnectionString, UriKind.Absolute, out var uri))
    {
        throw new InvalidOperationException(
            $"The gRPC URL '{grpcOptions.Value.IdentityServiceConnectionString}' is not a valid absolute URL. Please check your configuration.");
    }

    options.Address = uri; // url from options here
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();
app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();