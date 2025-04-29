using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthTools.Services;

internal static class TokenValidationParametersCreator
{
    public static TokenValidationParameters Create(IConfiguration configuration)
    {
        var jwtSecret = configuration["Jwt:secret"]!;
        var key = Encoding.UTF8.GetBytes(jwtSecret);
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = configuration["Jwt:issuer"],
            ValidAudience = configuration["Jwt:audience"],
            ClockSkew = TimeSpan.Zero,
        };
    }
}