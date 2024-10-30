using OrderService.Data;
using OrderService.Models;

namespace OrderService.Services;

public interface IOrderManager
{
    Task<long> CreateOrderAsync(CancellationToken cancellationToken);
}

public class OrderManager : IOrderManager
{
    private readonly AppDbContext _dbContext;

    public OrderManager(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<long> CreateOrderAsync(CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders.AddAsync(new OrderModel
        {
            CustomerId = 0,
            OrderDate = default,
            Status = OrderStatus.Pending,
            ShippingAddress = "Some shipping address",
            BillingAddress = "Some billing address",
            OrderItems = new List<OrderItemModel>
            {
                new OrderItemModel
                {
                    ProductId = Guid.NewGuid().ToString(),
                    Quantity = 1,
                    UnitPrice = 100,
                }
            }
        }, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _dbContext.Entry(order).ReloadAsync(cancellationToken);
        return order.Entity.Id;
    }
}