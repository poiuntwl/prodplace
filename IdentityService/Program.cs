using System.Threading.RateLimiting;
using IdentityService.Data;
using IdentityService.Extensions;
using IdentityService.Services;
using IdentityService.Services.grpc;
using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var s = builder.Services;
s.AddRateLimiter(o => o.AddFixedWindowLimiter(policyName: "fixed", x =>
{
    x.PermitLimit = 4;
    x.Window = TimeSpan.FromSeconds(12);
    x.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    x.QueueLimit = 2;
}));

s.AddKeycloakWebApiAuthentication(builder.Configuration, x =>
{
    x.TokenValidationParameters.ValidateAudience = true;
    x.TokenValidationParameters.ValidateIssuer = true;
});
s.AddKeycloakAuthorization();

s.AddAllServices(builder);
s.AddControllers();
s.AddEndpointsApiExplorer();
s.AddSwaggerGen();

builder.Services.AddHttpClient("keycloak-health",
    client => { client.BaseAddress = new Uri(builder.Configuration["Keycloak:ServerUrl"]); });

s.AddHealthChecks()
    .AddCheck<KeycloakHealthCheck>("keycloak");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

await ApplyMigrationsAsync();

app.MapControllers();
app.MapGrpcService<ValidationServiceGrpcServer>();
app.MapGrpcService<RoleAdminGrpcServer>();
app.Run();
return;

async Task ApplyMigrationsAsync()
{
    await using var serviceScope = app.Services.CreateAsyncScope();
    var dbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}