using IdentityService.Data;
using IdentityService.Services;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.BackgroundServices;

public class OutboxPublisher : BackgroundService
{
    private readonly AppDbContext _dbContext;
    private readonly IRabbitMqService _rabbitMqService;
    private readonly ILogger _logger;

    public OutboxPublisher(AppDbContext dbContext, IRabbitMqService rabbitMqService, ILoggerFactory loggerFactory)
    {
        _dbContext = dbContext;
        _rabbitMqService = rabbitMqService;
        _logger = loggerFactory.CreateLogger(GetType());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var outboxEvents = await _dbContext.OutboxMessages
                    .Where(x => x.ProcessedAt == null)
                    .OrderBy(x => x.CreatedAt)
                    .Take(100)
                    .ToListAsync(cancellationToken: stoppingToken);
                foreach (var e in outboxEvents)
                {
                    try
                    {
                        await _rabbitMqService.SendMessageAsync(e, "outboxQueue");
                        e.ProcessedAt = DateTime.UtcNow;
                        _dbContext.OutboxMessages.Update(e);
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
                await Task.Delay(2500, stoppingToken);
            }

            await _dbContext.SaveChangesAsync(stoppingToken);
        }
    }
}