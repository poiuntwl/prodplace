namespace ProductsService.Interfaces;

public interface IRabbitMqRpcClient
{
    Task<TResponse?> CallAsync<TRequest, TResponse>(TRequest request, string? queueName = null);
}