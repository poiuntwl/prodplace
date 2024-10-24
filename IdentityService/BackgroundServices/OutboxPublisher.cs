﻿using IdentityService.Data;
using IdentityService.Models;
using MassTransit;
using MessagingTools.Contracts;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.BackgroundServices;

public class OutboxPublisher : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly OutboxPublisherConfiguration _configuration;

    public OutboxPublisher(IServiceScopeFactory serviceScopeFactory,
        ILoggerFactory loggerFactory, OutboxPublisherConfiguration configuration)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
        _logger = loggerFactory.CreateLogger(GetType());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            try
            {
                var outboxEvents = await dbContext.OutboxMessages
                    .Where(x => x.ProcessedAt == null)
                    .OrderBy(x => x.CreatedAt)
                    .Take(100)
                    .ToListAsync(cancellationToken: stoppingToken);
                foreach (var e in outboxEvents)
                {
                    try
                    {
                        await publishEndpoint.Publish(new OutboxMessagePostedEvent
                        {
                            OutboxMessage = e
                        }, stoppingToken);
                        e.ProcessedAt = DateTime.UtcNow;
                        dbContext.OutboxMessages.Update(e);
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error publishing event. Id: {Id}. Message: {Message}", e.Id, ex.Message);
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error when publishing: {Message}", e.Message);
            }
            finally
            {
                await Task.Delay(_configuration.Delay, stoppingToken);
            }
        }
    }
}