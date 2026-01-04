extern alias ProductsServiceSUT;
using FluentAssertions;
using IntegrationTests.Factories;
using IntegrationTests.HttpClients;
using ProductsServiceSUT::ProductsService.Dtos.Product;

namespace IntegrationTests.Tests;

[Collection<BaseIntegrationCollection>]
public class ProductCrudIntegrationTests
{
    private readonly IProductServiceHttpClient _productServiceHttpClient;
    private readonly IIdentityServiceHttpClient _identityServiceHttpClient;

    public ProductCrudIntegrationTests(BaseIntegrationTestFixture baseIntegrationTestFixture)
    {
        _productServiceHttpClient = baseIntegrationTestFixture.ProductServiceFactory.HttpClient;
        _identityServiceHttpClient = baseIntegrationTestFixture.IdentityServiceFactory.HttpClient;
    }

    [Fact]
    public async Task GetAllProducts_ReturnsSeededProducts()
    {
        // Arrange - Register a user to get a valid JWT
        var user = await RegisterTestUserAsync();

        // Act
        var products = await _productServiceHttpClient.GetAll(user.Token);

        // Assert
        products.Should().NotBeNull();
        products.Should().HaveCountGreaterThanOrEqualTo(3, "because the factory seeds 3 products");
        products.Should().Contain(p => p.Name == "Laptop");
        products.Should().Contain(p => p.Name == "Smartphone");
        products.Should().Contain(p => p.Name == "Headphones");
    }

    [Fact]
    public async Task GetAllProducts_ContainsExpectedProductData()
    {
        // Arrange
        var user = await RegisterTestUserAsync();

        // Act
        var products = await _productServiceHttpClient.GetAll(user.Token);

        // Assert
        var laptop = products?.FirstOrDefault(p => p.Name == "Laptop");
        laptop.Should().NotBeNull();
        laptop!.Description.Should().Contain("16GB RAM");
        laptop.Price.Should().Be(1299.99m);
    }

    [Fact]
    public async Task CreateProduct_ReturnsNewProduct()
    {
        // Arrange
        var user = await RegisterTestUserAsync();
        var newProduct = new CreateProductRequestDto
        {
            Name = "Test Product " + Guid.NewGuid().ToString()[..8],
            Description = "A test product description",
            Price = 49.99m,
            CustomFields = "{\"color\": \"blue\"}"
        };

        // Act
        var result = await _productServiceHttpClient.Create(newProduct, user.Token);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(newProduct.Name);
        result.Description.Should().Be(newProduct.Description);
        result.Price.Should().Be(newProduct.Price);
    }

    [Fact]
    public async Task CreateProduct_ProductAppearsInGetAll()
    {
        // Arrange
        var user = await RegisterTestUserAsync();
        var productName = "Unique Product " + Guid.NewGuid();
        var newProduct = new CreateProductRequestDto
        {
            Name = productName,
            Description = "Description for unique product",
            Price = 199.99m,
            CustomFields = "{}"
        };

        // Act
        await _productServiceHttpClient.Create(newProduct, user.Token);
        var allProducts = await _productServiceHttpClient.GetAll(user.Token);

        // Assert
        allProducts.Should().Contain(p => p.Name == productName);
    }

    private async Task<IdentityService.Dtos.UserDataResult> RegisterTestUserAsync()
    {
        var registerDto = new IdentityService.Dtos.RegisterDto
        {
            Email = $"{Guid.NewGuid().ToString()[..8]}@test.com",
            Password = $"Test{Guid.NewGuid().ToString()[..8]}Password1!",
            FirstName = "Test",
            LastName = "User"
        };

        var response = await _identityServiceHttpClient.Register(registerDto);
        response.Should().NotBeNull("User registration should succeed");
        return response!;
    }
}
