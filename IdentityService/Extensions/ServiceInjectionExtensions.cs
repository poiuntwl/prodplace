using System.Reflection;
using AuthConfiguration;
using IdentityService.BackgroundServices;
using IdentityService.Data;
using IdentityService.Handlers.PostProcessors;
using IdentityService.Models;
using IdentityService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MessagingTools;
using Microsoft.Extensions.DependencyInjection.MessagingTools;

namespace IdentityService.Extensions;

public static class ServiceInjectionExtensions
{
    public static IServiceCollection AddAllServices(this IServiceCollection s, IConfiguration configuration)
    {
        s.AddIdentityServices(configuration);

        s.AddMediatR(x =>
        {
            x.RegisterServicesFromAssemblyContaining<Program>();

            x.AddRequestPostProcessor<RegisterUserPostProcessor>();
        });

        s.AddSingleton<OutboxPublisherConfiguration>(x => new OutboxPublisherConfiguration
        {
            Delay = TimeSpan.FromMilliseconds(int.TryParse(configuration["Outbox:DelayInMilliseconds"], out var delay)
                ? delay
                : 2500)
        });
        s.AddHostedService<OutboxPublisher>();
        s.AddTransient<ITokenService, TokenService>();
        s.AddScoped<IValidationService, ValidationService>();
        s.AddSingleton<RabbitMqSettings>(x => new RabbitMqSettings
        {
            QueueName = configuration["RabbitMq:QueueName"],
            HostName = configuration["RabbitMq:HostName"],
            Port = int.TryParse(configuration["RabbitMq:Port"], out var port) ? port : 5672,
            UserName = configuration["RabbitMq:UserName"],
            Password = configuration["RabbitMq:Password"],
        });
        s.AddScoped<IOutboxService, OutboxService>();
        s.AddGrpc(x => { x.EnableDetailedErrors = true; });
        s.AddMassTransitInjections(Assembly.GetExecutingAssembly());

        return s;
    }

    public static IServiceCollection AddIdentityServices(this IServiceCollection s, IConfiguration configuration)
    {
        s.AddDbContext<AppDbContext>(x =>
            x.UseSqlServer(configuration.GetConnectionString("IdentityConnection")));
        s.AddIdentity<AppUser, IdentityRole>(x =>
            {
                x.Password.RequireDigit = true;
                x.Password.RequiredLength = 8;
                x.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>();
        s.AddJwtAuthConfiguration(configuration);

        return s;
    }
}