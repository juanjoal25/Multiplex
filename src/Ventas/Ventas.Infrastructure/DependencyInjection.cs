using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ventas.Application.Abstractions;
using Ventas.Domain.Repositories;
using Ventas.Infrastructure.HttpClients;
using Ventas.Infrastructure.Messaging;
using Ventas.Infrastructure.Persistence;
using Ventas.Infrastructure.Schedulers;

namespace Ventas.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddVentasInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connStr = config.GetConnectionString("Postgres") ?? "Host=localhost;Port=5432;Database=multiplex_ventas;Username=multiplex;Password=multiplex";
        services.AddDbContext<VentasDbContext>(o => o.UseNpgsql(connStr));

        services.AddScoped<IOrdenRepository, OrdenRepository>();
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IDefComboRepository, DefComboRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        services.AddHttpClient<IClientesClient, ClientesHttpClient>(c => c.BaseAddress = new Uri(config["Services:Clientes"] ?? "http://localhost:5001"));
        services.AddHttpClient<IProgramacionClient, ProgramacionHttpClient>(c => c.BaseAddress = new Uri(config["Services:Programacion"] ?? "http://localhost:5002"));
        services.AddHttpClient<IInfraestructuraClient, InfraestructuraHttpClient>(c => c.BaseAddress = new Uri(config["Services:Infraestructura"] ?? "http://localhost:5003"));
        services.AddHttpClient<ICadenaClient, CadenaHttpClient>(c => c.BaseAddress = new Uri(config["Services:Cadena"] ?? "http://localhost:5006"));

        services.AddHostedService<OrdenExpirationScheduler>();

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumer<PagoAprobadoConsumer>();
            x.AddConsumer<PagoRechazadoConsumer>();
            x.AddConsumer<FuncionCanceladaConsumer>();
            x.AddEntityFrameworkOutbox<VentasDbContext>(o => { o.UsePostgres(); o.UseBusOutbox(); });
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
