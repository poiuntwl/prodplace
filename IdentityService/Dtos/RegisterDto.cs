using System.ComponentModel.DataAnnotations;

namespace IdentityService.Dtos;

public class RegisterDto
{
    [EmailAddress] public required string Email { get; set; }
    public required string Password { get; set; }
}