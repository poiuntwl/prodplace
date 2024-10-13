using MediatR;
using OrderService.Data;
using OrderService.Models;
using OrderService.Requests;

namespace OrderService.Handlers;

public class CreateOrderHandler : IRequestHandler<CreateOrderRequest>
{
    private readonly AppDbContext _dbContext;

    public CreateOrderHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        await _dbContext.Orders.AddAsync(new OrderModel
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

        // await _context.Entry(order).ReloadAsync();
        // return order;
    }
}