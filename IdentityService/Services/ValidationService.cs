using AuthTools.Services;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace IdentityService.Services;

public interface IValidationService
{
    Task<bool> ValidateRolesAsync(string token, string[]? roles, CancellationToken ct);
}

public class ValidationService : IValidationService
{
    private readonly IJwtClaimsPrincipalGetter _claimsPrincipalGetter;
    private readonly IKeycloakService _keycloakService;

    public ValidationService(IJwtClaimsPrincipalGetter claimsPrincipalGetter, IKeycloakService keycloakService)
    {
        _claimsPrincipalGetter = claimsPrincipalGetter;
        _keycloakService = keycloakService;
    }

    public async Task<bool> ValidateRolesAsync(string token, string[]? roles, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token) || roles == null)
        {
            return false;
        }

        try
        {
            var claimsPrincipal = _claimsPrincipalGetter.Get(token);

            var userId = claimsPrincipal?.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
            if (userId == null)
            {
                return false;
            }

            var userRoles = await _keycloakService.GetRolesForUserAsync(userId, ct);
            var rolesValid = roles.All(x => userRoles.Any(y => y.Name.Equals(x, StringComparison.OrdinalIgnoreCase)));

            return rolesValid;
        }
        catch (Exception)
        {
            return false;
        }
    }
}