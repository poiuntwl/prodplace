using AuthConfiguration;
using IdentityService.Data;
using IdentityService.Models;
using IdentityService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var s = builder.Services;
s.AddDbContext<AppDbContext>(x =>
    x.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")));
s.AddIdentity<AppUser, IdentityRole>(x =>
    {
        x.Password.RequireDigit = true;
        x.Password.RequiredLength = 8;
        x.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>();
s.AddJwtAuthConfiguration(builder.Configuration);

s.AddControllers();
s.AddEndpointsApiExplorer();
s.AddSwaggerGen();

s.AddScoped<ITokenService, TokenService>();
s.AddScoped<IValidationService, ValidationService>();
s.AddGrpc(x => { x.EnableDetailedErrors = true; });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

ApplyMigrationsAsync(app);

app.MapControllers();
app.MapGrpcService<ValidationServiceGrpcWrapper>();
app.Run();
return;

void ApplyMigrationsAsync(WebApplication app)
{
    using var serviceScope = app.Services.CreateScope();
    var dbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}