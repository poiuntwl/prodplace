using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthTools.Services;

internal static class TokenValidationParametersCreator
{
    public static TokenValidationParameters Create(IConfiguration configuration)
    {
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = false, // Allow RS256 tokens without public keys in dev
            ValidateIssuer = false,           // Allow differences between localhost and keycloak:8080
            ValidateAudience = false,         // Be permissive for dev access
            ClockSkew = TimeSpan.Zero,
            SignatureValidator = delegate(string token, TokenValidationParameters parameters)
            {
                var jwt = new JwtSecurityToken(token);
                return jwt;
            }
        };
    }
}