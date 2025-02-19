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
    public TokenValidationConfiguration(IConfiguration configuration)
    {
        ExpirationInSeconds = configuration.GetValue<int>("ExpirationInSeconds");
    }

    public TokenValidationParameters GetTokenValidationParameters()
    {
        return TokenValidationParametersCreator.Create();
    }

    public int ExpirationInSeconds { get; set; }
}