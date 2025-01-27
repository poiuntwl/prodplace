using System.Reflection;
using MassTransit;
using MessagingTools;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection.MessagingTools;

public static class MassTransitServiceInjectionExtensions
{
    public static IServiceCollection AddMassTransitInjections<TDbContext>(this IServiceCollection s, Assembly assembly,
        Action<IEntityFrameworkOutboxConfigurator>? configureOutbox = null)
        where TDbContext : DbContext
    {
        s.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumers(assembly);

            x.AddEntityFrameworkOutbox<TDbContext>(cfg =>
            {
                configureOutbox?.Invoke(cfg);
                cfg.QueryDelay = TimeSpan.FromSeconds(1);
            });

            x.UsingRabbitMq((ctx, cfg) =>
            {
                var rabbitMqSettings = ctx.GetRequiredService<RabbitMqSettings>();
                cfg.Host(new Uri(rabbitMqSettings.ConnectionString), y =>
                {
                    y.Username(rabbitMqSettings.UserName);
                    y.Password(rabbitMqSettings.Password);
                });

                cfg.ConfigureEndpoints(ctx);
            });
        });

        return s;
    }
}