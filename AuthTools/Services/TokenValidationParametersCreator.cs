using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthTools.Services;

internal static class TokenValidationParametersCreator
{
    public static TokenValidationParameters Create()
    {
        var config = LoadEmbeddedConfiguration();

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

    private static IConfiguration LoadEmbeddedConfiguration()
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string resourcePath = "AuthTools.shared.settings.Development.json";

        using var stream = assembly.GetManifestResourceStream(resourcePath);
        if (stream == null)
        {
            throw new FileNotFoundException(
                $"Configuration resource {resourcePath} not found. Make sure the file exists and is set as an Embedded Resource.");
        }
        
        return new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();
    }
}