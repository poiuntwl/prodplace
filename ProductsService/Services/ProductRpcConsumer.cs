using System.Text;
using System.Text.Json;
using ProductsService.Interfaces;
using ProductsService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ProductsService.Services;

public class ProductRpcConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly string _defaultQueueName;

    public ProductRpcConsumer(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
    {
        _defaultQueueName = configuration["RabbitMQ:DefaultQueueName"]!;
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"],
            UserName = configuration["RabbitMQ:UserName"],
            Password = configuration["RabbitMQ:Password"],
            Port = configuration.GetValue<int>("RabbitMQ:Port")
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _serviceScopeFactory = serviceScopeFactory;

        _channel.QueueDeclare(queue: _defaultQueueName, durable: false, exclusive: false, autoDelete: false,
            arguments: null);
        _channel.BasicQos(0, 1, false);
    }

    protected override Task ExecuteAsync(CancellationToken ct)
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (_, ea) =>
        {
            string response = null;
            var body = ea.Body.ToArray();
            var props = ea.BasicProperties;
            var replyProps = _channel.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;

            try
            {
                var typeHeader = GetHeaderValueAsString(props.Headers, "type");
                if (string.IsNullOrWhiteSpace(typeHeader))
                {
                    throw new ArgumentException("Type header is missing or invalid", nameof(props));
                }

                var requestType = Type.GetType(typeHeader)
                                  ?? throw new ArgumentException($"Invalid type: {typeHeader}", nameof(props));

                var message = Encoding.UTF8.GetString(body);
                var request = JsonSerializer.Deserialize(message, requestType)
                              ?? throw new JsonException("Deserialization resulted in null object");

                using var scope = _serviceScopeFactory.CreateScope();
                var router = scope.ServiceProvider.GetRequiredService<IProductRequestRouter>();
                var responseData = await router.RouteRequestAsync(request, ct);
                response = JsonSerializer.Serialize(responseData);
            }
            catch (Exception e)
            {
                response = JsonSerializer.Serialize(new { error = e.Message });
            }
            finally
            {
                var responseBytes = Encoding.UTF8.GetBytes(response ?? string.Empty);
                _channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps,
                    body: responseBytes);
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
        };

        _channel.BasicConsume(queue: _defaultQueueName, autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }

    public static string GetHeaderValueAsString(IDictionary<string, object> headers, string key)
    {
        if (headers.TryGetValue(key, out var value))
        {
            if (value is byte[] byteArray)
            {
                // Convert byte array to string
                return Encoding.UTF8.GetString(byteArray);
            }
            else if (value is string strValue)
            {
                // Value is already a string
                return strValue;
            }
            else
            {
                // For other types, convert to string representation
                return value?.ToString() ?? string.Empty;
            }
        }

        return string.Empty; // or throw an exception if the header must be present
    }
}