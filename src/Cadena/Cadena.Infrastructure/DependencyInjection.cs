using Cadena.Application.Abstractions;
using Cadena.Domain.Repositories;
using Cadena.Infrastructure.Messaging;
using Cadena.Infrastructure.Persistence;
using Cadena.Infrastructure.Schedulers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cadena.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCadenaInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connStr = config.GetConnectionString("Postgres") ?? "Host=localhost;Port=5432;Database=multiplex_cadena;Username=multiplex;Password=multiplex";
        services.AddDbContext<CadenaDbContext>(o => o.UseNpgsql(connStr));

        services.AddScoped<ISucursalRepository, SucursalRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
        services.AddHostedService<ContratoVigenciaScheduler>();

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddEntityFrameworkOutbox<CadenaDbContext>(o => { o.UsePostgres(); });
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
