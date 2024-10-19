using IdentityService.Data;
using IdentityService.Services;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.BackgroundServices;

public class OutboxPublisher : BackgroundService
{
    private readonly IRabbitMqService _rabbitMqService;
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public OutboxPublisher(IServiceScopeFactory serviceScopeFactory, IRabbitMqService rabbitMqService,
        ILoggerFactory loggerFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _rabbitMqService = rabbitMqService;
        _logger = loggerFactory.CreateLogger(GetType());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
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
                        await _rabbitMqService.SendMessageAsync(e, "outboxQueue");
                        e.ProcessedAt = DateTime.UtcNow;
                        dbContext.OutboxMessages.Update(e);
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

            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }
}