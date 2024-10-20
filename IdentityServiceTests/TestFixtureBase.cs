using Microsoft.Extensions.DependencyInjection;

namespace IdentityServiceTests;

public class TestFixtureBase : IAsyncLifetime
{
    private AsyncServiceScope ServiceScope { get; set; }
    protected readonly IServiceProvider ServiceProvider;

    protected TestFixtureBase(IdentityServiceFactory identityServiceFactory)
    {
        ServiceScope = identityServiceFactory.Services.CreateAsyncScope();
        ServiceProvider = ServiceScope.ServiceProvider;
    }

    public virtual Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public virtual async Task DisposeAsync()
    {
        await ServiceScope.DisposeAsync();
    }
}