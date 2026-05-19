using Clientes.Application.Abstractions;
using Clientes.Domain.Repositories;
using Clientes.Infrastructure.HttpClients;
using Clientes.Infrastructure.Messaging;
using Clientes.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Clientes.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddClientesInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connStr = config.GetConnectionString("Postgres") ?? "Host=localhost;Port=5432;Database=multiplex_clientes;Username=multiplex;Password=multiplex";
        services.AddDbContext<ClientesDbContext>(o => o.UseNpgsql(connStr));

        services.AddScoped<IEspectadorRepository, EspectadorRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        services.AddHttpClient<IFinancieroClient, FinancieroHttpClient>(c =>
        {
            var url = config["Services:Financiero"] ?? "http://localhost:5005";
            c.BaseAddress = new Uri(url);
        });

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddEntityFrameworkOutbox<ClientesDbContext>(o =>
            {
                o.UsePostgres();
            });
            x.UsingRabbitMq((ctx, cfg) =>
            {
                var host = config["RabbitMq:Host"] ?? "localhost";
                var user = config["RabbitMq:User"] ?? "multiplex";
                var pass = config["RabbitMq:Password"] ?? "multiplex";
                cfg.Host(host, "/", h => { h.Username(user); h.Password(pass); });
                cfg.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }
}
