using IdentityService.Data;
using IdentityService.Services;
using Microsoft.EntityFrameworkCore;

namespace IdentityService;

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
                    .ToListAsync(cancellationToken: stoppingToken);
                foreach (var e in outboxEvents)
                {
                    _rabbitMqService.SendMessage(e);
                    _dbContext.OutboxMessages.Remove(e);
                    await _dbContext.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error publishing outbox event: {Message}", e.Message);
            }
            finally
            {
                await Task.Delay(2500, stoppingToken);
            }
        }
    }
}