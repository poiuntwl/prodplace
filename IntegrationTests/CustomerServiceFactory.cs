using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using UserService;
using UserService.Data;

namespace IntegrationTests;

public class CustomerServiceFactory : WebApplicationFactory<IAppMarker>, IAsyncLifetime
{
    public HttpClient HttpClient = default!;
    private readonly PostgreSqlContainer _dbContainer;
    private NpgsqlConnection _sqlConnection = default!;
    private Respawner _respawner = default!;

    public CustomerServiceFactory(ContainersFactory containersFactory)
    {
        _dbContainer = containersFactory.CustomerDbContainer;
    }

    public async Task InitializeAsync()
    {
        HttpClient = CreateClient();
        await InitRespawnerAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices((_, s) =>
        {
            s.Remove(s.Single(x => x.ServiceType == typeof(DbContextOptions<AppDbContext>)));
            s.AddDbContext<AppDbContext>(y => { y.UseSqlServer(_dbContainer.GetConnectionString()); });
        });

        base.ConfigureWebHost(builder);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await ResetDbAsync();
        await _sqlConnection.DisposeAsync();
        await DisposeAsync();
    }

    private async Task InitRespawnerAsync()
    {
        _sqlConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());

        await _sqlConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_sqlConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }

    private async Task ResetDbAsync()
    {
        await _respawner.ResetAsync(_sqlConnection);
    }
}

[CollectionDefinition(nameof(CustomerServiceCollectionDefinition))]
public class CustomerServiceCollectionDefinition : ICollectionFixture<CustomerServiceFactory>;