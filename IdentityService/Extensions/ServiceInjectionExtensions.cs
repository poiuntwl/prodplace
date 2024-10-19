﻿using AuthConfiguration;
using IdentityService.BackgroundServices;
using IdentityService.Data;
using IdentityService.Handlers.Preprocessors;
using IdentityService.Models;
using IdentityService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Extensions;

public static class ServiceInjectionExtensions
{
    public static IServiceCollection AddAllServices(this IServiceCollection s, IConfiguration configuration)
    {
        s.AddIdentityServices(configuration);

        s.AddMediatR(x =>
        {
            x.RegisterServicesFromAssemblyContaining<Program>();
            x.AddRequestPostProcessor<RegisterUserMessagingPreprocessor>();
        });

        s.AddHostedService<OutboxPublisher>();
        s.AddTransient<ITokenService, TokenService>();
        s.AddScoped<IValidationService, ValidationService>();
        s.AddSingleton<IRabbitMqService, RabbitMqService>();
        s.AddScoped<IOutboxService, OutboxService>();
        s.AddGrpc(x => { x.EnableDetailedErrors = true; });

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