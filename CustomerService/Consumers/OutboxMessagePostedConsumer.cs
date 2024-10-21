using System.Text.Json;
using CommonModels.OutboxModels;
using MassTransit;
using MessagingTools.Contracts;
using UserService.Data;
using UserService.Models;

namespace UserService.Consumers;

public class OutboxMessagePostedConsumer : IConsumer<OutboxMessagePostedEvent>
{
    private readonly AppDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<OutboxMessagePostedConsumer> _logger;

    public OutboxMessagePostedConsumer(AppDbContext dbContext, IPublishEndpoint publishEndpoint,
        ILogger<OutboxMessagePostedConsumer> logger)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OutboxMessagePostedEvent> context)
    {
        var ct = context.CancellationToken;
        if (string.IsNullOrWhiteSpace(context.Message.OutboxMessage.Content))
        {
            return;
        }

        var content = JsonSerializer.Deserialize<UserCreatedEventData>(context.Message.OutboxMessage.Content);
        if (content == null)
        {
            return;
        }

        var newCustomer = new CustomerModel
        {
            FirstName = content.Username,
            LastName = string.Empty,
            Email = content.Email,
            PhoneNumber = "",
            DateOfBirth = default,
            CreatedAt = default,
            LastUpdated = null,
            Address = "",
            City = "",
            Country = "",
            PostalCode = ""
        };

        var createdCustomer = _dbContext.Customers.Add(newCustomer);
        await _dbContext.SaveChangesAsync(ct);
        await createdCustomer.ReloadAsync(ct);

        await _publishEndpoint.Publish(new CustomerCreatedEvent
        {
            ConsumerId = createdCustomer.Entity.Id,
            CreatedOnUtc = default
        }, ct);

        _logger.LogInformation("Created new customer: {Id}", createdCustomer.Entity.Id);
    }
}