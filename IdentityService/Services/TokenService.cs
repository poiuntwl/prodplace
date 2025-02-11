using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthTools.Models;
using AuthTools.Services;
using IdentityService.Dtos;
using IdentityService.Models;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Services;

public interface ITokenService
{
    string CreateToken(AppUser user);
    ClaimsPrincipal? ValidateToken(string token);
    string CreateToken(RegisterDto registerDto);
}

public class TokenService : ITokenService
{
    private readonly IJwtClaimsPrincipalGetter _claimsPrincipalGetter;
    private readonly ITokenValidationConfiguration _tokenValidationConfiguration;

    public TokenService(IJwtClaimsPrincipalGetter claimsPrincipalGetter,
        ITokenValidationConfiguration tokenValidationConfiguration)
    {
        _claimsPrincipalGetter = claimsPrincipalGetter;
        _tokenValidationConfiguration = tokenValidationConfiguration;
    }

    public string CreateToken(AppUser user)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(user.Email);
        ArgumentException.ThrowIfNullOrWhiteSpace(user.UserName);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.GivenName, user.UserName),
            new(JwtRegisteredClaimNames.Sub, user.Id)
        };

        var tokenValidationParameters = _tokenValidationConfiguration.GetTokenValidationParameters();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(7),
            SigningCredentials = new SigningCredentials(tokenValidationParameters.IssuerSigningKey,
                SecurityAlgorithms.Aes256Gcm),
            Issuer = tokenValidationParameters.ValidIssuer,
            Audience = tokenValidationParameters.ValidAudience,
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        return _claimsPrincipalGetter.Get(token);
    }

    public string CreateToken(RegisterDto registerDto)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(registerDto.Email);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Email, registerDto.Email),
            new(JwtRegisteredClaimNames.GivenName, registerDto.Email),
            new(JwtRegisteredClaimNames.Sub, registerDto.Email)
        };

        var tokenValidationParameters = _tokenValidationConfiguration.GetTokenValidationParameters();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(7),
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