using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthTools.Models;
using IdentityService.Dtos;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Services;

public interface ITokenService
{
    string CreateToken(CreateTokenDto createTokenDto);
}

public class TokenService : ITokenService
{
    private readonly ITokenValidationConfiguration _tokenValidationConfiguration;

    public TokenService(ITokenValidationConfiguration tokenValidationConfiguration)
    {
        _tokenValidationConfiguration = tokenValidationConfiguration;
    }

    public string CreateToken(CreateTokenDto createTokenDto)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(createTokenDto.Email);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Email, createTokenDto.Email),
            new(JwtRegisteredClaimNames.GivenName, createTokenDto.Email),
            new(JwtRegisteredClaimNames.Sub, createTokenDto.UserId)
        };

        var tokenValidationParameters = _tokenValidationConfiguration.GetTokenValidationParameters();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddSeconds(_tokenValidationConfiguration.ExpirationInSeconds),
            SigningCredentials = new SigningCredentials(tokenValidationParameters.IssuerSigningKey,
                SecurityAlgorithms.HmacSha256),
            Issuer = tokenValidationParameters.ValidIssuer,
            Audience = tokenValidationParameters.ValidAudience,
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}