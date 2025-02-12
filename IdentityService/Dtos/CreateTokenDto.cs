namespace IdentityService.Dtos;

public class CreateTokenDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public string UserId { get; set; }
}