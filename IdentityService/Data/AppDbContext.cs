using IdentityService.Constants;
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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("identity");
        var roles = new List<IdentityRole>
        {
            new()
            {
                Name = AppRoles.Admin,
                NormalizedName = AppRoles.Admin.ToUpper()
            },
            new()
            {
                Name = AppRoles.User,
                NormalizedName = AppRoles.User.ToUpper()
            },
        };
        builder.Entity<IdentityRole>().HasData(roles);
        builder.Entity<AppUser>().Property(x => x.LastLoginDate).HasDefaultValue(DateTime.UtcNow);
    }
}