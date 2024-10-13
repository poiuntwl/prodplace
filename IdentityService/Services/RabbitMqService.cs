using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace IdentityService.Services;

public interface IRabbitMqService : IDisposable
{
    void SendMessage<T>(T message);
}

public class RabbitMqService : IRabbitMqService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queueName;

    public RabbitMqService()
    {
        _queueName = "user-registrations";

        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "admin",
            Password = "password"
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: _queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    public void SendMessage<T>(T message)
    {
        var jsonMessage = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(jsonMessage);

        _channel.BasicPublish(exchange: "",
            routingKey: _queueName,
            basicProperties: null,
            body: body);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _channel.Close();
        _connection.Close();
    }
}