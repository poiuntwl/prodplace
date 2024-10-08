using IdentityGrpc.Server;

namespace ProductsService.Services;

public interface IAuthService
{
    Task<bool> ValidateTokenAsync(string token, CancellationToken ct);
    Task<bool> ValidateRolesAsync(string token, string[] requiredRoles, CancellationToken ct);
}

public class AuthService : IAuthService
{
    private readonly IdentityGrpc.Server.IdentityService.IdentityServiceClient _identityServiceClient;

    public AuthService(IdentityGrpc.Server.IdentityService.IdentityServiceClient identityServiceClient)
    {
        _identityServiceClient = identityServiceClient;
    }

    public async Task<bool> ValidateTokenAsync(string token, CancellationToken ct)
    {
        var validateResponse = await _identityServiceClient.ValidateTokenAsync(new ValidateTokenRequest
        {
            Token = token
        }, cancellationToken: ct);
        return validateResponse?.IsValid ?? false;
    }

    public async Task<bool> ValidateRolesAsync(string token, string[] requiredRoles, CancellationToken ct)
    {
        var validateResponse = await _identityServiceClient.ValidateRolesAsync(new ValidateRolesRequest
        {
            Token = token,
            Roles = { requiredRoles }
        }, cancellationToken: ct);

        return validateResponse?.IsValid ?? false;
    }
}