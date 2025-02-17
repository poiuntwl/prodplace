using System.Reflection;
using AuthTools;
using AuthTools.Models;
using IdentityService.Data;
using IdentityService.Handlers.PostProcessors;
using IdentityService.Services;
using Keycloak.AuthServices.Authorization;
using MassTransit;
using MessagingTools;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.MessagingTools;

namespace IdentityService.Extensions;

public static class ServiceInjectionExtensions
{
    public static void AddAllServices(this IServiceCollection s, WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        s.AddIdentityServices(builder);

        s.AddMediatR(x =>
        {
            x.RegisterServicesFromAssemblyContaining<Program>();

            x.AddRequestPostProcessor<RegisterUserPostProcessor>();
        });

        s.AddTransient<ITokenService, TokenService>();
        s.AddTransient<IValidationService, ValidationService>();
        s.AddSingleton<RabbitMqSettings>(x => new RabbitMqSettings
        {
            QueueName = configuration["RabbitMq:QueueName"],
            HostName = configuration["RabbitMq:HostName"],
            Port = int.TryParse(configuration["RabbitMq:Port"], out var port) ? port : 5672,
            UserName = configuration["RabbitMq:UserName"],
            Password = configuration["RabbitMq:Password"],
        });
        s.AddTransient<IOutboxService, OutboxService>();
        s.AddTransient<IUserService, UserService>();

        s.AddGrpc(x => { x.EnableDetailedErrors = true; });
        s.AddMassTransitInjections<AppDbContext>(Assembly.GetExecutingAssembly(), x => x.UseSqlServer());

        s.Configure<KeycloakConfigurationOptions>(configuration.GetSection("Keycloak"));
        s.AddKeycloakAuthorization(configuration);

        s.AddScoped<IKeycloakService, KeycloakService>();
        s.AddScoped<IRoleService, RoleService>();
    }

    private static IServiceCollection AddIdentityServices(this IServiceCollection s, WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        s.AddDbContext<AppDbContext>(x =>
            x.UseSqlServer(configuration.GetConnectionString("IdentityConnection")));
        s.AddJwtAuthServices();

        return s;
    }
}