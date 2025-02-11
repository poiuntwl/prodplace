extern alias AdminSUT;
using AdminSUT::Prodplace.Admin;
using AdminSUT::RoleAdmin;
using IntegrationTests.HttpClients;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Factories;

public class AdminServiceFactory : WebApplicationFactory<IAppMarker>, IAsyncLifetime
{
    public IServiceProvider ServiceProvider = default!;
    private IServiceScope _serviceScope = default!;
    public IAdminServiceHttpClient HttpClient = default!;
    private HttpMessageHandler _identityServiceHandler;

    public AdminServiceFactory(HttpMessageHandler identityServiceHandler)
    {
        _identityServiceHandler = identityServiceHandler;
    }

    public Task InitializeAsync()
    {
        _serviceScope = Services.CreateScope();
        ServiceProvider = _serviceScope.ServiceProvider;
        HttpClient = ServiceProvider.GetRequiredService<IAdminServiceHttpClient>();
        return Task.CompletedTask;
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        _serviceScope.Dispose();
        return Task.CompletedTask;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(x =>
        {
            x.AddHttpClient<IAdminServiceHttpClient, AdminServiceHttpClient>(_ =>
                new AdminServiceHttpClient(CreateClient()));

            var descriptor = x.FirstOrDefault(y => y.ServiceType == typeof(RoleAdminService.RoleAdminServiceClient));
            if (descriptor is not null)
            {
                x.Remove(descriptor);
            }

            x.AddGrpcClient<RoleAdminService.RoleAdminServiceClient>(o => { o.Address = new Uri("http://localhost"); })
                .ConfigurePrimaryHttpMessageHandler(() => _identityServiceHandler)
                .ConfigureChannel(y => y.HttpHandler = _identityServiceHandler);
        });

        base.ConfigureWebHost(builder);
    }
}