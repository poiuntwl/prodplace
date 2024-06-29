namespace ProductsService.Interfaces;

public interface IRabbitMQRpcClient
{
    Task<TResponse?> CallAsync<TRequest, TResponse>(TRequest request, string? queueName = null);
}