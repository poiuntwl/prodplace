using PriceService.Db;
using PriceService.Interfaces;
using PriceService.Repositories;
using ProdPlaceMongoDatabaseTools;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
var s = builder.Services;
s.AddControllers();
s.AddEndpointsApiExplorer();
s.AddSwaggerGen();

s.AddSingleton<MongoDbContext>(_ => new MongoDbContext(new MongoDbContextConfiguration
{
    ConnectionString = builder.Configuration.GetConnectionString("MongoDefaultConnection") ?? string.Empty,
    DatabaseName = builder.Configuration.GetValue<string>("MongoDb:DatabaseName") ?? string.Empty
}));
s.AddTransient<IPricesRepository, PricesRepository>();
s.AddMediatR(x => x.RegisterServicesFromAssemblyContaining<Program>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();