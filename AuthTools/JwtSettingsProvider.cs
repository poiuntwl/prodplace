using AuthTools.Models;
using Microsoft.Extensions.Configuration;

namespace AuthTools;

public static class JwtSettingsProvider
{
    private static IConfiguration? _configuration;

    public static void Initialize(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public static JwtSettings GetConfiguration()
    {
        if (_configuration == null)
        {
            throw new ApplicationException("Jwt configuration is not initialized.");
        }

        return _configuration.GetSection("Jwt").Get<JwtSettings>()!;
    }
}