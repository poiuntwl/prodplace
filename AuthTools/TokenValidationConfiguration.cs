using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthTools;

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