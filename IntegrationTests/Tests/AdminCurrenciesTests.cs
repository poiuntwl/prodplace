using FluentAssertions;
using IntegrationTests.Factories;
using IntegrationTests.HttpClients;

namespace IntegrationTests.Tests;

[Collection<BaseIntegrationCollection>]
public class AdminCurrenciesTests
{
    private readonly IAdminServiceHttpClient _adminServiceHttpClient;

    public AdminCurrenciesTests(BaseIntegrationTestFixture baseIntegrationTestFixture)
    {
        _adminServiceHttpClient = baseIntegrationTestFixture.AdminServiceFactory.HttpClient;
    }

    [Fact]
    public async Task ForceUpdate_ReturnsSuccessMessage()
    {
        var message = await _adminServiceHttpClient.ForceCurrencyUpdate();

        message.Should().Be("Update triggered successfully");
    }
}
