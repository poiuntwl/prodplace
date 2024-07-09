namespace ProductsService.Interfaces;

public interface IRabbitMqRpcClient
{
    Task<TResponse?> CallAsync<TRequest, TResponse>(TRequest request, CancellationToken ct) where TRequest : IQueueRequest<TResponse>;
    Task CallAsync<TRequest>(TRequest request, CancellationToken ct) where TRequest : IQueueRequest;
}