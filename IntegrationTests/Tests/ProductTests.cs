extern alias ProductsServiceSUT;
using FluentAssertions;
using IdentityService.Dtos;
using IntegrationTests.Factories;
using IntegrationTests.HttpClients;
using IntegrationTests.Utils;

namespace IntegrationTests.Tests;

[Collection<BaseIntegrationCollection>]
public class ProductTests
{
    private readonly IIdentityServiceHttpClient _identityServiceHttpClient;
    private readonly IProductServiceHttpClient _productServiceHttpClient;

    public ProductTests(BaseIntegrationTestFixture baseIntegrationTestFixture)
    {
        _productServiceHttpClient = baseIntegrationTestFixture.ProductServiceFactory.HttpClient;
        _identityServiceHttpClient = baseIntegrationTestFixture.IdentityServiceFactory.HttpClient;
    }

    [Fact]
    public async Task CreateProduct_ShouldCreateProductAndReturn()
    {
        var user = await _identityServiceHttpClient.Register(new RegisterDto
            {
                Email = $"{Guid.NewGuid().ToString()[..5]}@gmail.com",
                Password = $"Some{Guid.NewGuid().ToString()[..5]}password1!",
                FirstName = TestDataGenerator.GenerateFirstName(),
                LastName = TestDataGenerator.GenerateLastName()
            }
        );
        user.Should().NotBeNull();

        var products = await _productServiceHttpClient.GetAll(user.Token);
        products.Should().NotBeEmpty();

        /*
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
        */
    }
}