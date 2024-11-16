using AuthTools.Constants;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Data;

public class AppDbContext : IdentityDbContext<AppUser>
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
                Name = RoleNames.Admin,
                NormalizedName = RoleNames.Admin.ToUpper()
            },
            new()
            {
                Name = RoleNames.User,
                NormalizedName = RoleNames.User.ToUpper()
            },
        };
        builder.Entity<IdentityRole>().HasData(roles);
        builder.Entity<AppUser>().Property(x => x.LastLoginDate).HasDefaultValueSql("GETUTCDATE()");
    }
}