using MediatR;
using OrderService.Dto;
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
        await _orderManager.CreateOrderAsync(request.CreateOrderDto.CustomerId, request.CreateOrderDto.ShippingAddress,
            request.CreateOrderDto.BillingAddress, request.CreateOrderDto.OrderItems, cancellationToken);
    }
}

public record CreateOrderRequest(CreateOrderDto CreateOrderDto) : IRequest;