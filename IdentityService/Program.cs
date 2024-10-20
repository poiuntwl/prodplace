using IdentityService.Data;
using IdentityService.Extensions;
using IdentityService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var s = builder.Services;
s.AddAllServices(builder.Configuration);
s.AddControllers();
s.AddEndpointsApiExplorer();
s.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

ApplyMigrations();

app.MapControllers();
app.MapGrpcService<ValidationServiceGrpcWrapper>();
app.Run();
return;

void ApplyMigrations()
{
    using var serviceScope = app.Services.CreateScope();
    var dbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}