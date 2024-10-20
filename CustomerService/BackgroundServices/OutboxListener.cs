using System.Text;
using System.Text.Json;
using CommonModels.OutboxModels;
using IdentityService.Models;
using MassTransit;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MessagingTools;
using MessagingTools.Contracts;
using UserService.Data;
using UserService.Models;

namespace UserService.BackgroundServices;

public class OutboxListener : BackgroundService
{
    private readonly IModel _channel;
    private readonly ILogger<OutboxListener> _logger;
    private readonly string _queueName;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IPublishEndpoint _publishEndpoint;

    public OutboxListener(RabbitMqSettings rabbitMqSettings, ILoggerFactory loggerFactory,
        IServiceScopeFactory serviceScopeFactory, IPublishEndpoint publishEndpoint)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _publishEndpoint = publishEndpoint;

        _logger = loggerFactory.CreateLogger<OutboxListener>();

        _queueName = rabbitMqSettings.QueueName;

        var factory = new ConnectionFactory
        {
            HostName = rabbitMqSettings.HostName,
            Port = rabbitMqSettings.Port,
            UserName = rabbitMqSettings.UserName,
            Password = rabbitMqSettings.Password,
        };
        var connection = factory.CreateConnection();
        _channel = connection.CreateModel();

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
            var serviceScope = _serviceScopeFactory.CreateScope();
            var dbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            _logger.LogInformation("Received message: {Message}", message);

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
                LastName = string.Empty,
                Email = userCreatedResult.Email,
                PhoneNumber = "",
                DateOfBirth = default,
                CreatedAt = default,
                LastUpdated = null,
                Address = "",
                City = "",
                Country = "",
                PostalCode = ""
            };

            var createdCustomer = dbContext.Customers.Add(newCustomer);
            await dbContext.SaveChangesAsync(stoppingToken);
            await createdCustomer.ReloadAsync(stoppingToken);

            await _publishEndpoint.Publish(new CustomerCreatedEvent
            {
                ConsumerId = createdCustomer.Entity.Id,
                CreatedOnUtc = default
            }, stoppingToken);

            _logger.LogInformation("Created new customer: {Id}", createdCustomer.Entity.Id);

            await dbContext.DisposeAsync();
            serviceScope.Dispose();
        };

        _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
        return Task.CompletedTask;
    }
}