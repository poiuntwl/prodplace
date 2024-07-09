using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using ProductsService.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ProductsService.Services;

public class RabbitMQRpcClient : IRabbitMqRpcClient, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly EventingBasicConsumer _consumer;
    private readonly BlockingCollection<string> _respQueue = new();
    private readonly IBasicProperties _props;
    private readonly string? _defaultQueueName;

    public RabbitMQRpcClient(IConfiguration configuration)
    {
        _defaultQueueName = configuration["RabbitMQ:DefaultQueueName"];
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"],
            UserName = configuration["RabbitMQ:UserName"],
            Password = configuration["RabbitMQ:Password"],
            Port = int.Parse(configuration["RabbitMQ:Port"]!),
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        var replyQueueName = _channel.QueueDeclare().QueueName;
        _consumer = new EventingBasicConsumer(_channel);

        _props = _channel.CreateBasicProperties();
        _props.Headers ??= new Dictionary<string, object>();
        var correlationId = Guid.NewGuid().ToString();
        _props.CorrelationId = correlationId;
        _props.ReplyTo = replyQueueName;

        _consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var response = Encoding.UTF8.GetString(body);
            if (ea.BasicProperties.CorrelationId == correlationId)
            {
                _respQueue.Add(response);
            }
        };

        _channel.BasicConsume(
            consumer: _consumer,
            queue: replyQueueName,
            autoAck: true);
    }

    public async Task<TResponse?> CallAsync<TRequest, TResponse>(TRequest request, string? queueName = null)
    {
        queueName ??= _defaultQueueName;
        var message = JsonSerializer.Serialize(request);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        _props.Headers["type"] = typeof(TRequest).ToString();
        _channel.BasicPublish(
            exchange: "",
            routingKey: queueName,
            basicProperties: _props,
            body: messageBytes);

        var response = _respQueue.Take();
        return JsonSerializer.Deserialize<TResponse>(response);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _channel.Dispose();
        _connection.Dispose();
    }
}