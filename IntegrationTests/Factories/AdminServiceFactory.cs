extern alias AdminSUT;
using AdminSUT::Prodplace.Admin;
using AdminSUT::RoleAdmin;
using IntegrationTests.HttpClients;
using IntegrationTests.Utils;
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

    public AdminServiceFactory(HttpMessageHandler httpMessageHandler)
    {
        _identityServiceHandler = httpMessageHandler;
    }

    public ValueTask InitializeAsync()
    {
        _serviceScope = Services.CreateScope();
        ServiceProvider = _serviceScope.ServiceProvider;
        HttpClient = ServiceProvider.GetRequiredService<IAdminServiceHttpClient>();
        return default;
    }

    public override ValueTask DisposeAsync()
    {
        _serviceScope.Dispose();
        return default;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(x =>
        {
            x.AddHttpClient<IAdminServiceHttpClient, AdminServiceHttpClient>(_ =>
                new AdminServiceHttpClient(CreateClient()));

            var currencyHandler = new StubHttpMessageHandler(_ => StubHttpMessageHandler.Ok());
            x.AddSingleton<IHttpClientFactory>(new StubHttpClientFactory(currencyHandler));

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
