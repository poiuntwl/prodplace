using System.Text;
using System.Text.Json;
using CommonModels.OutboxModels;
using IdentityService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UserService.Data;
using UserService.Models;

namespace UserService.BackgroundServices;

public class OutboxListener : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queueName;
    private readonly ILogger<OutboxListener> _logger;
    private readonly AppDbContext _dbContext;

    public OutboxListener(IConfiguration configuration, ILoggerFactory loggerFactory, AppDbContext dbContext)
    {
        _dbContext = dbContext;
        _logger = loggerFactory.CreateLogger<OutboxListener>();

        _queueName = configuration["RabbitMq:QueueName"] ?? "outboxQueue";

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

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            _logger.LogInformation($"Received message: {message}");

            var outboxEventEntity = JsonSerializer.Deserialize<OutboxMessage>(message);
            if (outboxEventEntity == null)
            {
                return;
            }

            var userCreatedResult = JsonSerializer.Deserialize<UserCreatedEventData>(outboxEventEntity.Content);
            if (userCreatedResult == null)
            {
                return;
            }

            var newCustomer = new CustomerModel
            {
                FirstName = userCreatedResult.Username,
                LastName = null,
                Email = userCreatedResult.Email,
                PhoneNumber = null,
                DateOfBirth = default,
                CreatedAt = default,
                LastUpdated = null,
                Address = null,
                City = null,
                Country = null,
                PostalCode = null
            };

            var createdCustomer = _dbContext.Customers.Add(newCustomer);

            _logger.LogInformation("Created new customer: {Id}", createdCustomer.Entity.Id);

            await _dbContext.SaveChangesAsync(stoppingToken);
        };

        _channel.BasicConsume(queue: "orderEventsQueue", autoAck: true, consumer: consumer);
        return Task.CompletedTask;
    }
}