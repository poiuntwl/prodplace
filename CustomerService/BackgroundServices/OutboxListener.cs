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
    private readonly IModel _channel;
    private readonly ILogger<OutboxListener> _logger;
    private readonly AppDbContext _dbContext;
    private readonly IServiceScope _serviceScope;
    private string _queueName;

    public OutboxListener(RabbitMqSettings rabbitMqSettings, ILoggerFactory loggerFactory,
        IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScope = serviceScopeFactory.CreateScope();

        _dbContext = _serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
        _logger = loggerFactory.CreateLogger<OutboxListener>();

        _queueName = rabbitMqSettings.QueueName ?? "outboxQueue";

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

        _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _serviceScope.Dispose();
        GC.SuppressFinalize(this);
        base.Dispose();
    }
}