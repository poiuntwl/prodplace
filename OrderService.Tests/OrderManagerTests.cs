using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using OrderService.Services;
using Xunit;

namespace OrderService.Tests;

public class OrderManagerTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly OrderManager _sut;

    public OrderManagerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _sut = new OrderManager(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task CreateOrderAsync_CreatesOrderWithPendingStatus()
    {
        // Arrange
        var customerId = 1;
        var shippingAddress = "123 Shipping St";
        var billingAddress = "456 Billing Ave";
        var orderItems = new List<OrderItemModel>
        {
            new() { ProductId = "prod123", Quantity = 2, UnitPrice = 10.00m }
        };

        // Act
        var orderId = await _sut.CreateOrderAsync(customerId, shippingAddress, billingAddress, orderItems, CancellationToken.None);

        // Assert
        orderId.Should().BeGreaterThan(0);
        var createdOrder = await _dbContext.Orders.FindAsync(orderId);
        createdOrder.Should().NotBeNull();
        createdOrder!.Status.Should().Be(OrderStatus.Pending);
        createdOrder.CustomerId.Should().Be(customerId);
        createdOrder.ShippingAddress.Should().Be(shippingAddress);
        createdOrder.BillingAddress.Should().Be(billingAddress);
    }

    [Fact]
    public async Task UpdateOrderAsync_UpdatesAddresses()
    {
        // Arrange
        var order = new OrderModel
        {
            CustomerId = 1,
            OrderDate = DateTimeOffset.UtcNow,
            Status = OrderStatus.Pending,
            ShippingAddress = "Old Shipping",
            BillingAddress = "Old Billing",
            OrderItems = new List<OrderItemModel>()
        };
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        var newShippingAddress = "New Shipping St";
        var newBillingAddress = "New Billing Ave";

        // Act
        await _sut.UpdateOrderAsync(order.Id, newShippingAddress, newBillingAddress, CancellationToken.None);

        // Assert
        var updatedOrder = await _dbContext.Orders.FindAsync(order.Id);
        updatedOrder!.ShippingAddress.Should().Be(newShippingAddress);
        updatedOrder.BillingAddress.Should().Be(newBillingAddress);
    }

    [Fact]
    public async Task UpdateOrderAsync_ThrowsWhenOrderNotFound()
    {
        // Arrange
        var nonExistentOrderId = 99999L;

        // Act
        var act = () => _sut.UpdateOrderAsync(nonExistentOrderId, "New Shipping", "New Billing", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Order {nonExistentOrderId} not found.");
    }

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Processing)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Processing, OrderStatus.Shipped)]
    [InlineData(OrderStatus.Processing, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Delivered)]
    public async Task ChangeOrderStatusAsync_ValidTransition_UpdatesStatus(OrderStatus fromStatus, OrderStatus toStatus)
    {
        // Arrange
        var order = new OrderModel
        {
            CustomerId = 1,
            OrderDate = DateTimeOffset.UtcNow,
            Status = fromStatus,
            ShippingAddress = "Shipping",
            BillingAddress = "Billing",
            OrderItems = new List<OrderItemModel>()
        };
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        // Act
        await _sut.ChangeOrderStatusAsync(order.Id, toStatus, CancellationToken.None);

        // Assert
        var updatedOrder = await _dbContext.Orders.FindAsync(order.Id);
        updatedOrder!.Status.Should().Be(toStatus);
    }

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Shipped)]
    [InlineData(OrderStatus.Pending, OrderStatus.Delivered)]
    [InlineData(OrderStatus.Processing, OrderStatus.Pending)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Processing)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Pending)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Pending)]
    public async Task ChangeOrderStatusAsync_InvalidTransition_ThrowsException(OrderStatus fromStatus, OrderStatus toStatus)
    {
        // Arrange
        var order = new OrderModel
        {
            CustomerId = 1,
            OrderDate = DateTimeOffset.UtcNow,
            Status = fromStatus,
            ShippingAddress = "Shipping",
            BillingAddress = "Billing",
            OrderItems = new List<OrderItemModel>()
        };
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        // Act
        var act = () => _sut.ChangeOrderStatusAsync(order.Id, toStatus, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Invalid status transition from {fromStatus} to {toStatus}");
    }

    [Fact]
    public async Task GetOrderAsync_ReturnsOrder_WhenExists()
    {
        // Arrange
        var order = new OrderModel
        {
            CustomerId = 1,
            OrderDate = DateTimeOffset.UtcNow,
            Status = OrderStatus.Pending,
            ShippingAddress = "Shipping",
            BillingAddress = "Billing",
            OrderItems = new List<OrderItemModel>
            {
                new() { ProductId = "prod1", Quantity = 1, UnitPrice = 10m }
            }
        };
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetOrderAsync(order.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
        result.OrderItems.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetOrderAsync_ReturnsNull_WhenDeleted()
    {
        // Arrange
        var order = new OrderModel
        {
            CustomerId = 1,
            OrderDate = DateTimeOffset.UtcNow,
            Status = OrderStatus.Pending,
            ShippingAddress = "Shipping",
            BillingAddress = "Billing",
            IsDeleted = true,
            OrderItems = new List<OrderItemModel>()
        };
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetOrderAsync(order.Id, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetOrdersByCustomerAsync_ReturnsOnlyCustomerOrders()
    {
        // Arrange
        var customerId = 1;
        var otherCustomerId = 2;
        
        _dbContext.Orders.AddRange(
            new OrderModel { CustomerId = customerId, Status = OrderStatus.Pending, ShippingAddress = "S1", BillingAddress = "B1", OrderItems = new List<OrderItemModel>() },
            new OrderModel { CustomerId = customerId, Status = OrderStatus.Processing, ShippingAddress = "S2", BillingAddress = "B2", OrderItems = new List<OrderItemModel>() },
            new OrderModel { CustomerId = otherCustomerId, Status = OrderStatus.Pending, ShippingAddress = "S3", BillingAddress = "B3", OrderItems = new List<OrderItemModel>() }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetOrdersByCustomerAsync(customerId, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(o => o.CustomerId == customerId);
    }

    [Fact]
    public async Task SoftDeleteOrderAsync_SetsIsDeletedFlag()
    {
        // Arrange
        var order = new OrderModel
        {
            CustomerId = 1,
            OrderDate = DateTimeOffset.UtcNow,
            Status = OrderStatus.Pending,
            ShippingAddress = "Shipping",
            BillingAddress = "Billing",
            IsDeleted = false,
            OrderItems = new List<OrderItemModel>()
        };
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        // Act
        await _sut.SoftDeleteOrderAsync(order.Id, CancellationToken.None);

        // Assert
        var deletedOrder = await _dbContext.Orders.FindAsync(order.Id);
        deletedOrder!.IsDeleted.Should().BeTrue();
    }
}
