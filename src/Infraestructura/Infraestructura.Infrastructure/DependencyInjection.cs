using Infraestructura.Application.Abstractions;
using Infraestructura.Domain.Repositories;
using Infraestructura.Infrastructure.Messaging;
using Infraestructura.Infrastructure.Persistence;
using Infraestructura.Infrastructure.Schedulers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infraestructura.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfraestructuraInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connStr = config.GetConnectionString("Postgres") ?? "Host=localhost;Port=5432;Database=multiplex_infraestructura;Username=multiplex;Password=multiplex";
        services.AddDbContext<InfraestructuraDbContext>(o => o.UseNpgsql(connStr));

        services.AddScoped<ISalaRepository, SalaRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        services.AddHostedService<ReservaExpirationScheduler>();

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumer<OrdenCreadaConsumer>();
            x.AddConsumer<OrdenExpiradaConsumer>();
            x.AddConsumer<OrdenCanceladaConsumer>();
            x.AddConsumer<FuncionCanceladaConsumer>();
            x.AddEntityFrameworkOutbox<InfraestructuraDbContext>(o => { o.UsePostgres(); o.UseBusOutbox(); });
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
