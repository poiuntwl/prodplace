using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using AuthTools.Constants;
using AuthTools.Middleware;
using AuthTools.Models;
using AuthTools.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthTools;

public static class JwtDependencyInjectionExtensions
{
    public static IServiceCollection AddJwtAuthConfiguration(this IServiceCollection s, IConfiguration config)
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        s.Configure<JwtSettings>(config.GetSection("Jwt"));

        s.AddAuthorizationBuilder()
            .AddPolicy(PolicyNames.RequireAdminRole, builder => builder.RequireRole(RoleNames.Admin))
            .AddPolicy(PolicyNames.RequireUserRole, builder => builder.RequireRole(RoleNames.User));

        s.AddAdminAuthorizationOverride();

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
                x.Events = new JwtBearerEvents
                {
                    OnTokenValidated = ctx =>
                    {
                        if (ctx.Principal == null)
                        {
                            return Task.CompletedTask;
                        }

                        var realmRoles = ctx.Principal.Claims
                            .Where(c => c.Type == "realm_access")
                            .Select(c => c.Value)
                            .FirstOrDefault();

                        if (string.IsNullOrEmpty(realmRoles))
                        {
                            return Task.CompletedTask;
                        }

                        var roles = JsonSerializer.Deserialize<Dictionary<string, string[]>>(realmRoles);
                        if (roles == null)
                        {
                            return Task.CompletedTask;
                        }

                        var claimsIdentity = ctx.Principal.Identity as ClaimsIdentity;

                        if (roles.TryGetValue("roles", out var value) == false)
                        {
                            return Task.CompletedTask;
                        }

                        foreach (var role in value)
                        {
                            claimsIdentity?.AddClaim(new Claim(ClaimTypes.Role, role));
                        }

                        return Task.CompletedTask;
                    }
                };

                x.RequireHttpsMetadata = false;
                x.Audience = config["Jwt:audience"];
                x.MetadataAddress = config["Jwt:metadataAddress"];
                x.TokenValidationParameters = TokenValidationParametersCreator.Create(config);
            });

        return s;
    }

    public static void AddJwtAuthServices(this IServiceCollection s)
    {
        s.AddSingleton<ITokenValidationConfiguration, TokenValidationConfiguration>();
        s.AddSingleton<IJwtClaimsPrincipalGetter, JwtClaimsPrincipalGetter>();
        s.AddSingleton<IJwtValidator, JwtValidator>();
    }

    public static WebApplication UseJwtAuthConfiguration(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    public static IServiceCollection AddAdminAuthorizationOverride(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, AdminAuthorizationHandler>();

        return services;
    }
}