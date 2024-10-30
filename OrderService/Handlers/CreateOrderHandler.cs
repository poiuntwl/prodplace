using MediatR;
using OrderService.Requests;
using OrderService.Services;

namespace OrderService.Handlers;

public class CreateOrderHandler : IRequestHandler<CreateOrderRequest>
{
    private readonly IOrderManager _orderManager;

    public CreateOrderHandler(IOrderManager orderManager)
    {
        _orderManager = orderManager;
    }

    public async Task Handle(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        await _orderManager.CreateOrderAsync(cancellationToken);
    }
}