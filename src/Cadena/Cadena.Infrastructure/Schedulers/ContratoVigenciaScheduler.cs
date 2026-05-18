using Cadena.Application.Abstractions;
using Cadena.Domain.Events;
using Cadena.Domain.Repositories;
using Cadena.Domain.Services;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cadena.Infrastructure.Schedulers;

public sealed class ContratoVigenciaScheduler(IServiceProvider sp, ILogger<ContratoVigenciaScheduler> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = sp.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<ISucursalRepository>();
                var svc = scope.ServiceProvider.GetRequiredService<SvcGestionContratos>();
                var pub = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var sucursales = await repo.GetTodasAsync(stoppingToken);
                svc.VerificarVigencias(sucursales, DateTime.UtcNow);

                foreach (var s in sucursales)
                {
                    foreach (var e in s.DomainEvents.OfType<ContratoCorporativoVencido>())
                        await pub.Publish(new global::Messaging.Contracts.Cadena.ContratoCorporativoVencido(
                            e.IdContrato.Value, e.IdSucursal.Value, e.OccurredOn), stoppingToken);
                    s.ClearEvents();
                    await repo.UpdateAsync(s, stoppingToken);
                }
                await uow.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex) { log.LogError(ex, "Error ContratoVigenciaScheduler"); }
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
