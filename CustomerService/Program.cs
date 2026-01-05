using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using MessagingTools;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection.MessagingTools;
using UserService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
var s = builder.Services;
s.AddEndpointsApiExplorer();
s.AddSwaggerGen();
s.AddDbContext<AppDbContext>(x =>
    x.UseNpgsql(builder.Configuration.GetConnectionString("DbConnection")));


s.AddSingleton<RabbitMqSettings>(sp => {
    var config = sp.GetRequiredService<IConfiguration>();
    var section = config.GetSection("RabbitMq");
    return new RabbitMqSettings
    {
        HostName = section["HostName"],
        Port = int.TryParse(section["Port"], out var p) ? p : 5672,
        UserName = section["UserName"],
        Password = section["Password"],
        QueueName = section["QueueName"] ?? "CustomerServiceQueue"
    };
});
s.AddMassTransitInjections<AppDbContext>(Assembly.GetExecutingAssembly(), x => x.UsePostgres());

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

ApplyMigrations();

app.Run();
return;

void ApplyMigrations()
{
    using var serviceScope = app.Services.CreateScope();
    var dbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

public class Interceptor : IInterceptor
{
}