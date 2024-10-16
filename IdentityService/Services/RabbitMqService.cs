using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace IdentityService.Services;

public interface IRabbitMqService : IDisposable
{
    Task SendMessageAsync<T>(T message, string? queueName = null);
}

public class RabbitMqService : IRabbitMqService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queueName;

    public RabbitMqService(IConfiguration configuration)
    {
        _queueName = configuration["RabbitMq:QueueName"] ?? "eventsQueue";

        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMq:HostName"],
            Port = int.TryParse(configuration["RabbitMq:Port"], out var port) ? port : 5672,
            UserName = configuration["RabbitMq:UserName"],
            Password = configuration["RabbitMq:Password"],
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    public Task SendMessageAsync<T>(T message, string? queueName = null)
    {
        var jsonMessage = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(jsonMessage);

        _channel.BasicPublish(exchange: "",
            routingKey: queueName ?? _queueName,
            basicProperties: null,
            body: body);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _channel.Close();
        _connection.Close();
    }
}