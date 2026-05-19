using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Programacion.Application.Abstractions;
using Programacion.Domain.Repositories;
using Programacion.Infrastructure.HttpClients;
using Programacion.Infrastructure.Messaging;
using Programacion.Infrastructure.Persistence;

namespace Programacion.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramacionInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connStr = config.GetConnectionString("Postgres") ?? "Host=localhost;Port=5432;Database=multiplex_programacion;Username=multiplex;Password=multiplex";
        services.AddDbContext<ProgramacionDbContext>(o => o.UseNpgsql(connStr));

        services.AddScoped<IPeliculaRepository, PeliculaRepository>();
        services.AddScoped<IFuncionRepository, FuncionRepository>();
        services.AddScoped<ICarteleraRepository, CarteleraRepository>();
        services.AddScoped<IAlquilerRepository, AlquilerRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        services.AddHttpClient<IInfraestructuraClient, InfraestructuraHttpClient>(c =>
            c.BaseAddress = new Uri(config["Services:Infraestructura"] ?? "http://localhost:5003"));

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumer<SalaEnMantenimientoConsumer>();
            x.AddConsumer<SalaReactivadaConsumer>();
            x.AddEntityFrameworkOutbox<ProgramacionDbContext>(o => { o.UsePostgres(); });
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
