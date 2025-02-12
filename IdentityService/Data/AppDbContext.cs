using AuthTools.Constants;
using IdentityService.Models;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Data;

public class AppDbContext : IdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("identity");
        var roles = new List<IdentityRole>
        {
            new()
            {
                Id = "18ac2c02-a136-4bd2-adfc-ff97d6472f25",
                Name = RoleNames.Admin,
                NormalizedName = RoleNames.Admin.ToUpper()
            },
            new()
            {
                Id = "782ff602-7c26-4b17-9f06-3aec93c9ea84",
                Name = RoleNames.User,
                NormalizedName = RoleNames.User.ToUpper()
            },
        };
        builder.Entity<IdentityRole>().HasData(roles);

        builder.AddInboxStateEntity();
        builder.AddOutboxMessageEntity();
        builder.AddOutboxStateEntity();
    }
}