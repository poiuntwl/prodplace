namespace IntegrationTests;

[Collection(nameof(ContainersFactoryCollectionDefinition))]
public class RegisterTests : IClassFixture<IdentityServiceFactory>, IClassFixture<CustomerServiceFactory>
{
    private HttpClient _identityHttpClient;
    private HttpClient _customerHttpClient;

    public RegisterTests(IdentityServiceFactory identityServiceFactory, CustomerServiceFactory customerServiceFactory)
    {
        _identityHttpClient = identityServiceFactory.HttpClient;
        _customerHttpClient = customerServiceFactory.HttpClient;
    }

    [Fact]
    public void Test1()
    {
    }
}