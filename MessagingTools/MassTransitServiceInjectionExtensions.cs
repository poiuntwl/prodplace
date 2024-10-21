using System.Reflection;
using MassTransit;
using MessagingTools;

namespace Microsoft.Extensions.DependencyInjection.MessagingTools;

public static class MassTransitServiceInjectionExtensions
{
    public static IServiceCollection AddMassTransitInjections(this IServiceCollection s, Assembly assembly)
    {
        s.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.AddConsumers(assembly);

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