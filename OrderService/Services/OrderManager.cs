using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Services;

public interface IOrderManager
{
    Task<long> CreateOrderAsync(int customerId, string shippingAddress, string billingAddress,
        ICollection<OrderItemModel> orderItems, CancellationToken cancellationToken);

    Task UpdateOrderAsync(long orderId, string shippingAddress, string billingAddress,
        CancellationToken cancellationToken);

    Task ChangeOrderStatusAsync(long orderId, OrderStatus newStatus, CancellationToken cancellationToken);
    Task<OrderModel?> GetOrderAsync(long orderId, CancellationToken cancellationToken);
    Task<List<OrderModel>> GetOrdersByCustomerAsync(int customerId, CancellationToken cancellationToken);
    Task SoftDeleteOrderAsync(long orderId, CancellationToken cancellationToken);
}

public class OrderManager : IOrderManager
{
    private static readonly Dictionary<OrderStatus, HashSet<OrderStatus>> ValidTransitions = new()
    {
        [OrderStatus.Pending] = [OrderStatus.Processing, OrderStatus.Cancelled],
        [OrderStatus.Processing] = [OrderStatus.Shipped, OrderStatus.Cancelled],
        [OrderStatus.Shipped] = [OrderStatus.Delivered],
        [OrderStatus.Delivered] = [],
        [OrderStatus.Cancelled] = []
    };


    private readonly AppDbContext _dbContext;

    public OrderManager(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<long> CreateOrderAsync(int customerId, string shippingAddress, string billingAddress,
        ICollection<OrderItemModel> orderItems, CancellationToken cancellationToken)
    {
        var order = new OrderModel
        {
            CustomerId = customerId,
            OrderDate = DateTimeOffset.UtcNow,
            Status = OrderStatus.Pending,
            ShippingAddress = shippingAddress,
            BillingAddress = billingAddress,
            OrderItems = orderItems
        };

        await _dbContext.Orders.AddAsync(order, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return order.Id;
    }

    public async Task UpdateOrderAsync(long orderId, string shippingAddress, string billingAddress,
        CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders.FindAsync([orderId], cancellationToken);
        if (order is null) throw new KeyNotFoundException($"Order {orderId} not found.");

        order.ShippingAddress = shippingAddress;
        order.BillingAddress = billingAddress;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ChangeOrderStatusAsync(long orderId, OrderStatus newStatus, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders.FindAsync([orderId], cancellationToken);
        if (order is null) throw new KeyNotFoundException($"Order {orderId} not found.");

        if (!ValidTransitions[order.Status].Contains(newStatus))
            throw new InvalidOperationException($"Invalid status transition from {order.Status} to {newStatus}");

        order.Status = newStatus;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<OrderModel?> GetOrderAsync(long orderId, CancellationToken cancellationToken)
        => await _dbContext.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted, cancellationToken);

    public async Task<List<OrderModel>> GetOrdersByCustomerAsync(int customerId, CancellationToken cancellationToken)
        => await _dbContext.Orders
            .Where(o => o.CustomerId == customerId && !o.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task SoftDeleteOrderAsync(long orderId, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders.FindAsync([orderId], cancellationToken);
        if (order is null) throw new KeyNotFoundException($"Order {orderId} not found.");

        order.IsDeleted = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}