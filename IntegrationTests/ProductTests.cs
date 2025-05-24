extern alias ProductsServiceSUT;
using System.Text.Json;
using FluentAssertions;
using IdentityService.Dtos;
using IntegrationTests.Factories;
using IntegrationTests.HttpClients;
using ProductsServiceSUT::ProductsService.Dtos.Product;

namespace IntegrationTests;

[Collection<IntegrationCollection>]
public class ProductTests
{
    private readonly IIdentityServiceHttpClient _identityServiceHttpClient;
    private readonly IProductServiceHttpClient _productServiceHttpClient;

    public ProductTests(IntegrationTestFixture integrationTestFixture)
    {
        _productServiceHttpClient = integrationTestFixture.ProductServiceFactory.HttpClient;
        _identityServiceHttpClient = integrationTestFixture.IdentityServiceFactory.HttpClient;
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