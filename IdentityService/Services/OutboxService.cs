using System.Text.Json;
using System.Transactions;
using IdentityService.Data;
using IdentityService.Models;

namespace IdentityService.Services;

public interface IOutboxService
{
    Task CreateOutboxMessageAsync(string messageType, object content, CancellationToken ct);
}

public class OutboxService : IOutboxService
{
    private readonly AppDbContext _dbContext;

    public OutboxService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateOutboxMessageAsync(string messageType, object content, CancellationToken ct)
    {
        using var txs = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var msg = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = messageType,
            Content = JsonSerializer.Serialize(content),
            CreatedAt = DateTime.UtcNow,
        };

        await _dbContext.OutboxMessages.AddAsync(msg, ct);
        await _dbContext.SaveChangesAsync(ct);
        txs.Complete();
    }
}