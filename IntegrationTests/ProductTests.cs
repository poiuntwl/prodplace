using System.Text.Json;
using FluentAssertions;
using IdentityService.Dtos;
using IntegrationTests.Factories;
using IntegrationTests.HttpClients;
using ProductsService.Dtos.Product;

namespace IntegrationTests;

[Collection(nameof(ContainersFactoryCollectionDefinition))]
public class ProductTests : IClassFixture<ProductServiceFactory>, IClassFixture<IdentityServiceFactory>
{
    private readonly IProductServiceHttpClient _productServiceHttpClient;
    private readonly IIdentityServiceHttpClient _identityServiceHttpClient;

    public ProductTests(ProductServiceFactory productServiceFactory, IdentityServiceFactory identityServiceFactory)
    {
        _productServiceHttpClient = productServiceFactory.HttpClient;
        _identityServiceHttpClient = identityServiceFactory.HttpClient;
    }

    [Fact]
    public async Task CreateProduct_ShouldCreateProductAndReturn()
    {
        var user = await _identityServiceHttpClient.Register(new RegisterDto
        {
            Username = "someusername",
            Email = "someusername@gmail.com",
            Password = "Somevalidpassword1!"
        });
        user.Should().NotBeNull();

        await _productServiceHttpClient.Create(new CreateProductRequestDto
        {
            Name = "Some name",
            Description = "Some description",
            Price = 123,
            CustomFields = JsonSerializer.Serialize(new
            {
                Hello = "World"
            })
        }, user.Token);

        var all = await _productServiceHttpClient.GetAll(user.Token);
    }
}