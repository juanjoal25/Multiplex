using Financiero.Application.Abstractions;
using Financiero.Domain.Repositories;
using Financiero.Infrastructure.Messaging;
using Financiero.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Financiero.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFinancieroInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connStr = config.GetConnectionString("Postgres") ?? "Host=localhost;Port=5432;Database=multiplex_financiero;Username=multiplex;Password=multiplex";
        services.AddDbContext<FinancieroDbContext>(o => o.UseNpgsql(connStr));

        services.AddScoped<ITransaccionRepository, TransaccionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
        services.AddScoped<IPasarelaClient, StubPasarelaClient>();

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumer<OrdenConfirmadaConsumer>();
            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(config["RabbitMq:Host"] ?? "localhost", "/", h =>
                {
                    h.Username(config["RabbitMq:User"] ?? "multiplex");
                    h.Password(config["RabbitMq:Password"] ?? "multiplex");
                });
                cfg.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }
}
