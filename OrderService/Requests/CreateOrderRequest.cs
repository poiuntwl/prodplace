using MediatR;
using OrderService.Dto;

namespace OrderService.Requests;

public record CreateOrderRequest(CreateOrderDto CreateOrderDto) : IRequest;