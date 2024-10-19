using System.Text.Json;
using IdentityService.Data;
using IdentityService.Models;
using IdentityService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServiceTests;

public class UnitTests : IClassFixture<IdentityServiceFactory>
{
    private IServiceProvider ServiceProvider { get; set; }

    public UnitTests(IdentityServiceFactory factory)
    {
        ServiceProvider = factory.Services;
    }

    [Fact]
    public async Task Test1()
    {
        using var scope = ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<AppDbContext>();
        dbContext.OutboxMessages.Add(new OutboxMessage
        {
            Type = "sometype",
            Content = JsonSerializer.Serialize(new { Hello = "world" }),
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = null
        });
        await dbContext.SaveChangesAsync();
        var result = await dbContext.OutboxMessages.ToListAsync();
        // var outboxService = scope.ServiceProvider.GetService<IOutboxService>();
        // await outboxService.CreateOutboxMessageAsync("identity.registerUser", 1, CancellationToken.None);
    }
}