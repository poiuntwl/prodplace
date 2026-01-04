using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthTools.Services;

internal static class TokenValidationParametersCreator
{
    public static TokenValidationParameters Create(IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"];
        SecurityKey? signingKey = null;
        
        if (!string.IsNullOrEmpty(jwtKey))
        {
            signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        }
        
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = false, // Allow RS256 tokens without public keys in dev
            ValidateIssuer = false,           // Allow differences between localhost and keycloak:8080
            ValidateAudience = false,         // Be permissive for dev access
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = signingKey,
            SignatureValidator = delegate(string token, TokenValidationParameters parameters)
            {
                var jwt = new JwtSecurityToken(token);
                return jwt;
            }
        };
    }
}