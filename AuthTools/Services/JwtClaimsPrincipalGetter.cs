using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthTools.Models;
using Microsoft.IdentityModel.Tokens;

namespace AuthTools.Services;

public interface IJwtClaimsPrincipalGetter
{
    ClaimsPrincipal? Get(string token);
}

public class JwtClaimsPrincipalGetter : IJwtClaimsPrincipalGetter
{
    private readonly TokenValidationParameters _validationParameters;

    public JwtClaimsPrincipalGetter(ITokenValidationConfiguration tokenValidationConfiguration)
    {
        _validationParameters = tokenValidationConfiguration.GetTokenValidationParameters();
    }

    public ClaimsPrincipal? Get(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var claimsPrincipal = tokenHandler.ValidateToken(token, _validationParameters, out _);

            return claimsPrincipal;
        }
        catch (Exception)
        {
            return null;
        }
    }
}