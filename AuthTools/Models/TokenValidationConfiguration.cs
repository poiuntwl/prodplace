using AuthTools.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthTools.Models;

public interface ITokenValidationConfiguration
{
    TokenValidationParameters GetTokenValidationParameters();
}

public class TokenValidationConfiguration : ITokenValidationConfiguration
{
    private readonly IConfiguration _configuration;

    public TokenValidationConfiguration(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public TokenValidationParameters GetTokenValidationParameters()
    {
        return TokenValidationParametersCreator.Create(_configuration);
    }
}