using AuthTools.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthTools.Models;

public interface ITokenValidationConfiguration
{
    TokenValidationParameters GetTokenValidationParameters();
    public int ExpirationInSeconds { get; set; }
}

public class TokenValidationConfiguration : ITokenValidationConfiguration
{
    private readonly IConfiguration _configuration;

    public TokenValidationConfiguration(IConfiguration configuration)
    {
        _configuration = configuration;
        ExpirationInSeconds = _configuration.GetValue<int>("ExpirationInSeconds");
    }

    public TokenValidationParameters GetTokenValidationParameters()
    {
        return TokenValidationParametersCreator.Create(_configuration);
    }

    public int ExpirationInSeconds { get; set; }
}