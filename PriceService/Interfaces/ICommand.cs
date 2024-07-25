using MediatR;

namespace PriceService.Interfaces;

public interface ICommand : IRequest;
public interface ICommand<out TResponse> : IRequest<TResponse>;