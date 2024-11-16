using AuthTools.Services;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace IdentityService.Services;

public interface IValidationService
{
    Task<bool> ValidateRolesAsync(string token, string[]? roles);
}

public class ValidationService : IValidationService
{
    private readonly IJwtClaimsPrincipalGetter _claimsPrincipalGetter;
    private readonly UserManager<AppUser> _userManager;

    public ValidationService(UserManager<AppUser> userManager, IJwtClaimsPrincipalGetter claimsPrincipalGetter)
    {
        _userManager = userManager;
        _claimsPrincipalGetter = claimsPrincipalGetter;
    }

    public async Task<bool> ValidateRolesAsync(string token, string[]? roles)
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

            var user = _userManager.Users.FirstOrDefault(x => x.Id == userId);
            if (user == null)
            {
                return false;
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var rolesValid = roles.All(x => userRoles.Contains(x));

            return rolesValid;
        }
        catch (Exception)
        {
            return false;
        }
    }
}