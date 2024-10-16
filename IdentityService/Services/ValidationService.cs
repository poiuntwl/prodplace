using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace IdentityService.Services;

public interface IValidationService
{
    bool ValidateToken(string token);
    Task<bool> ValidateRolesAsync(string token, string[]? roles);
}

public class ValidationService : IValidationService
{
    private readonly ITokenService _tokenService;
    private readonly UserManager<AppUser> _userManager;

    public ValidationService(ITokenService tokenService, UserManager<AppUser> userManager)
    {
        _tokenService = tokenService;
        _userManager = userManager;
    }

    public bool ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            var claimsPrincipal = _tokenService.ValidateToken(token);
            return claimsPrincipal != null;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public async Task<bool> ValidateRolesAsync(string token, string[]? roles)
    {
        if (string.IsNullOrWhiteSpace(token) || roles == null)
        {
            return false;
        }

        try
        {
            var claimsPrincipal = _tokenService.ValidateToken(token);

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