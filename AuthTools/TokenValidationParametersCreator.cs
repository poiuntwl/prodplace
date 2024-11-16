using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthTools;

internal static class TokenValidationParametersCreator
{
    public static TokenValidationParameters Create(IConfiguration config)
    {
        var jwtSecret = config["Jwt:secret"]!;
        var key = Encoding.UTF8.GetBytes(jwtSecret);
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = config["Jwt:issuer"],
            ValidAudience = config["Jwt:audience"],
            ClockSkew = TimeSpan.Zero,
        };
    }
}