using Microsoft.AspNetCore.Identity;

namespace IdentityService.Models;

public class AppUser : IdentityUser
{
    public DateTime LastLoginDate { get; set; }
}