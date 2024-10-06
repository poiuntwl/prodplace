using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace AuthConfiguration;

public static class JwtExtensions
{
    public static IServiceCollection AddJwtAuthConfiguration(this IServiceCollection s, ConfigurationManager config)
    {
        var jwtSecret = config["jwt:secret"]!;
        var key = Encoding.UTF8.GetBytes(jwtSecret);

        s.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(x =>
            {
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = config["jwt:issuer"],
                    ValidAudience = config["jwt:audience"],
                };
            });

        return s;
    }

    public static WebApplication UseJwtAuthConfiguration(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }
}