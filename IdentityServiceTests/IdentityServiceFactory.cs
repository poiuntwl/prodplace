﻿using System.Data.Common;
using IdentityService;
using IdentityService.Data;
using IdentityService.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RabbitMqTools;
using Respawn;
using Testcontainers.MsSql;

namespace IdentityServiceTests;

public class IdentityServiceFactory : WebApplicationFactory<IAppMarker>, IAsyncLifetime
{
    public HttpClient HttpClient = default!;


    private readonly MsSqlContainer _dbContainer;
    private DbConnection _dbConnection = default!;
    private Respawner _respawner = default!;
    private readonly Mock<IRabbitMqService> _rabbitMqServiceMock = new();

    public IdentityServiceFactory()
    {
        _dbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-CU13-ubuntu-22.04")
            .WithCleanUp(true)
            .Build();

        _rabbitMqServiceMock.Setup(x => x.SendMessageAsync(It.IsAny<object>(), It.IsAny<string>()));
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        HttpClient = CreateClient();
        await InitRespawner();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices((_, s) =>
        {
            s.Remove(s.Single(x => x.ServiceType == typeof(DbContextOptions<AppDbContext>)));
            s.AddDbContext<AppDbContext>(y => { y.UseSqlServer(_dbContainer.GetConnectionString()); });
            s.Remove(s.Single(x => x.ServiceType == typeof(IRabbitMqService)));
            s.AddSingleton(_rabbitMqServiceMock.Object);
        });

        base.ConfigureWebHost(builder);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbConnection.DisposeAsync();
        await _dbContainer.DisposeAsync();
        await DisposeAsync();
    }

    public async Task ResetDbAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }

    private async Task InitRespawner()
    {
        _dbConnection = new SqlConnection(_dbContainer.GetConnectionString());
        await _dbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude = new[] { "dbo" }
        });
    }
}

[CollectionDefinition(nameof(IdentityServiceCollectionDefinition))]
public class IdentityServiceCollectionDefinition : ICollectionFixture<IdentityServiceFactory>;