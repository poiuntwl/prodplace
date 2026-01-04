using FluentAssertions;
using MongoDB.Bson;
using NSubstitute;
using ProductsService.Dtos.Product;
using ProductsService.Interfaces;
using ProductsService.Models.MongoDbModels;
using ProductsService.Services;
using Xunit;

namespace ProductsService.Tests;

public class ProductManagerTests
{
    private readonly IProductRepository _mockRepository;
    private readonly ProductManager _sut;

    public ProductManagerTests()
    {
        _mockRepository = Substitute.For<IProductRepository>();
        _sut = new ProductManager(_mockRepository);
    }

    [Fact]
    public async Task GetProductAsync_ReturnsProduct_WhenExists()
    {
        // Arrange
        var productId = ObjectId.GenerateNewId();
        var expectedProduct = new ProductModel
        {
            Id = productId,
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            CustomFields = "{}"
        };
        _mockRepository.GetProductAsync(productId, Arg.Any<CancellationToken>())
            .Returns(expectedProduct);

        // Act
        var result = await _sut.GetProductAsync(productId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Product");
        result.Description.Should().Be("Test Description");
        result.Price.Should().Be(99.99m);
    }

    [Fact]
    public async Task GetProductAsync_ReturnsNull_WhenNotExists()
    {
        // Arrange
        var productId = ObjectId.GenerateNewId();
        _mockRepository.GetProductAsync(productId, Arg.Any<CancellationToken>())
            .Returns((ProductModel?)null);

        // Act
        var result = await _sut.GetProductAsync(productId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProductsAsync_ReturnsAllProducts()
    {
        // Arrange
        var products = new List<ProductModel>
        {
            new() { Id = ObjectId.GenerateNewId(), Name = "Product 1", Price = 10m, CustomFields = "{}" },
            new() { Id = ObjectId.GenerateNewId(), Name = "Product 2", Price = 20m, CustomFields = "{}" },
            new() { Id = ObjectId.GenerateNewId(), Name = "Product 3", Price = 30m, CustomFields = "{}" }
        };
        _mockRepository.GetProductsAsync(Arg.Any<CancellationToken>())
            .Returns(products);

        // Act
        var result = await _sut.GetProductsAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Select(p => p.Name).Should().Contain(new[] { "Product 1", "Product 2", "Product 3" });
    }

    [Fact]
    public async Task CreateProductAsync_ReturnsNewId()
    {
        // Arrange
        var expectedId = ObjectId.GenerateNewId();
        var product = new ProductModel
        {
            Name = "New Product",
            Description = "New Description",
            Price = 50m
        };
        _mockRepository.CreateProductAsync(product, Arg.Any<CancellationToken>())
            .Returns(expectedId);

        // Act
        var result = await _sut.CreateProductAsync(product, CancellationToken.None);

        // Assert
        result.Should().Be(expectedId);
        await _mockRepository.Received(1).CreateProductAsync(product, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdatePriceAsync_CallsRepository()
    {
        // Arrange
        var productId = ObjectId.GenerateNewId();
        var newPrice = 149.99m;

        // Act
        await _sut.UpdatePriceAsync(productId, newPrice, CancellationToken.None);

        // Assert
        await _mockRepository.Received(1).UpdatePriceAsync(productId, newPrice, Arg.Any<CancellationToken>());
    }
}
