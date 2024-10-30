using MediatR;
using OrderService.Requests;
using OrderService.Services;

namespace OrderService.Handlers;

public class CreateOrderHandler : IRequestHandler<CreateOrderRequest>
{
    private readonly IOrderService _orderService;

    public CreateOrderHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task Handle(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        await _orderService.CreateOrderAsync(cancellationToken);
    }
}