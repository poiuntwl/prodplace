using AuthTools;
using FluentValidation;
using ProductsService;
using ProductsService.Handlers.PreProcessors;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var s = builder.Services;
s.AddControllers();
s.AddEndpointsApiExplorer();
s.AddSwaggerGen();
s.AddDbServices(builder);
s.AddHealthChecks();
s.AddJwtAuthConfiguration(builder);
s.AddJwtAuthServices();
s.AddGrpcClient<IdentityGrpc.Server.IdentityService.IdentityServiceClient>(o =>
{
    o.Address = new Uri("https://identity-service");
});

s.AddMediatR(x =>
{
    x.RegisterServicesFromAssemblyContaining<Program>();
    x.AddRequestPreProcessor<CreateProductHandlerPreProcessor>();
});
s.AddValidatorsFromAssemblyContaining<Program>();

s.AddProductServices();
s.AddValidationMiddleware();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();
// app.UseValidationMiddleware();

app.Run();