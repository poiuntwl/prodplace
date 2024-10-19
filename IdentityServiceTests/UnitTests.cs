using System.Text.Json;
using IdentityService.Data;
using IdentityService.Models;
using IdentityService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServiceTests;

public class UnitTests : TestFixtureBase, IClassFixture<IdentityServiceFactory>
{
    public UnitTests(IdentityServiceFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Test1()
    {
        var dbContext = ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.OutboxMessages.Add(new OutboxMessage
        {
            Type = "sometype",
            Content = JsonSerializer.Serialize(new { Hello = "world" }),
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = null
        });
        await dbContext.SaveChangesAsync();
        var result = await dbContext.OutboxMessages.ToListAsync();
        var outboxService = ServiceProvider.GetRequiredService<IOutboxService>();
        await outboxService.CreateOutboxMessageAsync("identity.registerUser", 1, CancellationToken.None);
    }
}