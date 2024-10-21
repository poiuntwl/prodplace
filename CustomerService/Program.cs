using System.Reflection;
using Microsoft.EntityFrameworkCore;
using MessagingTools;
using Microsoft.Extensions.DependencyInjection.MessagingTools;
using UserService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
var s = builder.Services;
s.AddEndpointsApiExplorer();
s.AddSwaggerGen();
s.AddDbContext<AppDbContext>(x =>
    x.UseNpgsql(builder.Configuration.GetConnectionString("DbConnection")));

s.AddSingleton<RabbitMqSettings>();
s.AddMassTransitInjections(Assembly.GetExecutingAssembly());

var app = builder.Build();

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