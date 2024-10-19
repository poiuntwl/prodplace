using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace AuthConfiguration;

public static class JwtDependenyInjectionExtensions
{
    public static IServiceCollection AddJwtAuthConfiguration(this IServiceCollection s, IConfiguration config)
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        var jwtSecret = config["Jwt:secret"]!;
        var key = Encoding.UTF8.GetBytes(jwtSecret);

        s.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme =
                    x.DefaultChallengeScheme =
                        x.DefaultForbidScheme =
                            x.DefaultScheme =
                                x.DefaultSignInScheme =
                                    x.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = config["Jwt:issuer"],
                    ValidAudience = config["Jwt:audience"],
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