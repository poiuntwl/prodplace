using FluentAssertions;
using NSubstitute;
using PriceService.Interfaces;
using PriceService.Services;
using Xunit;

namespace PriceService.Tests;

public class PriceManagerTests
{
    private readonly IPricesRepository _mockRepository;
    private readonly PriceManager _sut;

    public PriceManagerTests()
    {
        _mockRepository = Substitute.For<IPricesRepository>();
        _sut = new PriceManager(_mockRepository);
    }

    [Fact]
    public async Task SetPriceAsync_ReturnsTrue_WhenSuccessful()
    {
        // Arrange
        var productId = 123;
        var price = 99.99m;
        _mockRepository.UpdatePriceAsync(productId, price, Arg.Any<CancellationToken>(), Arg.Any<bool>())
            .Returns(true);

        // Act
        var result = await _sut.SetPriceAsync(productId, price, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        await _mockRepository.Received(1).UpdatePriceAsync(productId, price, Arg.Any<CancellationToken>(), Arg.Any<bool>());
    }

    [Fact]
    public async Task SetPriceAsync_ReturnsFalse_WhenRepositoryFails()
    {
        // Arrange
        var productId = 123;
        var price = 99.99m;
        _mockRepository.UpdatePriceAsync(productId, price, Arg.Any<CancellationToken>(), Arg.Any<bool>())
            .Returns(false);

        // Act
        var result = await _sut.SetPriceAsync(productId, price, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999999)]
    public async Task SetPriceAsync_HandlesVariousProductIds(int productId)
    {
        // Arrange
        var price = 50.00m;
        _mockRepository.UpdatePriceAsync(productId, price, Arg.Any<CancellationToken>(), Arg.Any<bool>())
            .Returns(true);

        // Act
        var result = await _sut.SetPriceAsync(productId, price, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(999.99)]
    [InlineData(9999999.99)]
    public async Task SetPriceAsync_HandlesVariousPrices(decimal price)
    {
        // Arrange
        var productId = 1;
        _mockRepository.UpdatePriceAsync(productId, price, Arg.Any<CancellationToken>(), Arg.Any<bool>())
            .Returns(true);

        // Act
        var result = await _sut.SetPriceAsync(productId, price, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }
}
