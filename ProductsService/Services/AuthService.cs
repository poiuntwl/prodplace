namespace ProductsService.Services;

public interface IAuthService
{
    Task<bool> ValidateRolesAsync(string token, string[] requiredRoles, CancellationToken ct);
}

public class AuthService : IAuthService
{
    private readonly AuthHttpClient _httpClient;

    public AuthService(AuthHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> ValidateRolesAsync(string token, string[] requiredRoles, CancellationToken ct)
    {
        return await _httpClient.ValidateRolesAsync(token, requiredRoles, ct);
    }
}