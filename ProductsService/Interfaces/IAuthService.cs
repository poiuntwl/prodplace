namespace ProductsService.Interfaces;

public interface IAuthService
{
    Task<bool> ValidateTokenAsync(string token, CancellationToken ct);
    Task<bool> ValidateRolesAsync(string token, string[] requiredRoles, CancellationToken ct);
}