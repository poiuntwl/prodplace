using AuthTools;
using IdentityGrpc.Server;
using ProductsService.Interfaces;
using static IdentityGrpc.Server.IdentityService;

namespace ProductsService.Services;

public class AuthService : IAuthService
{
    private readonly IdentityServiceClient _identityServiceClient;
    private readonly IJwtValidator _validator;

    public AuthService(IJwtValidator validator, IdentityServiceClient identityServiceClient)
    {
        _validator = validator;
        _identityServiceClient = identityServiceClient;
    }

    public Task<bool> ValidateTokenAsync(string token, CancellationToken ct)
    {
        return Task.FromResult(_validator.Validate(token));
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