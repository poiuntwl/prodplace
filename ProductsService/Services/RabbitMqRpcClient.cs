using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using ProductsService.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ProductsService.Services;

public class RabbitMqRpcClient : IRabbitMqRpcClient, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly EventingBasicConsumer _consumer;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _callbackMapper = new();
    private readonly IBasicProperties _props;
    private readonly string? _defaultQueueName;

    public RabbitMqRpcClient(IAppConfigurationManager configuration)
    {
        _defaultQueueName = configuration.RabbitMqDefaultQueueName;
        var factory = new ConnectionFactory
        {
            HostName = configuration.RabbitMqHostName,
            UserName = configuration.RabbitMqUserName,
            Password = configuration.RabbitMqPassword,
            Port = configuration.RabbitMqPort,
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        var replyQueueName = _channel.QueueDeclare().QueueName;
        _consumer = new EventingBasicConsumer(_channel);

        _props = _channel.CreateBasicProperties();
        _props.Headers ??= new Dictionary<string, object>();

        _consumer.Received += (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var response = Encoding.UTF8.GetString(body);
            var correlationId = ea.BasicProperties.CorrelationId;
            if (_callbackMapper.TryRemove(correlationId, out var tcs))
            {
                tcs.TrySetResult(response);
            }
        };
    }

    public async Task<TResponse?> CallAsync<TRequest, TResponse>(TRequest request, CancellationToken ct)
        where TRequest : IQueueRequest<TResponse>
    {
        var response = await SendRequestAsync(request);
        return JsonSerializer.Deserialize<TResponse>(response);
    }

    public Task CallAsync<TRequest>(TRequest request, CancellationToken ct) where TRequest : IQueueRequest
    {
        throw new NotImplementedException();
    }

    private async Task<string> SendRequestAsync<TRequest>(TRequest request)
    {
        var message = JsonSerializer.Serialize(request);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        var correlationId = Guid.NewGuid().ToString();
        _props.CorrelationId = correlationId;

        _props.Headers["type"] = typeof(TRequest).ToString();

        var tcs = new TaskCompletionSource<string>();
        _callbackMapper[correlationId] = tcs;

        _channel.BasicPublish(
            exchange: "",
            routingKey: _defaultQueueName,
            basicProperties: _props,
            body: messageBytes);

        return await tcs.Task;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _channel.Dispose();
        _connection.Dispose();
    }
}