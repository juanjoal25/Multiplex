using Infraestructura.Application.Abstractions;
using Infraestructura.Domain.Events;
using Infraestructura.Domain.Repositories;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infraestructura.Infrastructure.Schedulers;

public sealed class ReservaExpirationScheduler(
    IServiceProvider sp,
    ILogger<ReservaExpirationScheduler> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = sp.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<ISalaRepository>();
                var publisher = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var ahora = DateTime.UtcNow;
                var salas = await repo.GetConReservasExpiradasAsync(ahora, stoppingToken);
                foreach (var sala in salas)
                {
                    var ids = sala.LiberarExpiradas(ahora);
                    if (ids.Count == 0) continue;
                    await repo.UpdateAsync(sala, stoppingToken);
                    foreach (var e in sala.DomainEvents.OfType<SillaLiberada>())
                    {
                        await publisher.Publish(new global::Messaging.Contracts.Infraestructura.SillaLiberada(
                            e.IdSilla.Value, e.IdFuncion, e.Motivo, e.OccurredOn), stoppingToken);
                        await publisher.Publish(new global::Messaging.Contracts.Infraestructura.ReservaExpirada(
                            e.IdSilla.Value, Guid.Empty, e.OccurredOn), stoppingToken);
                    }
                    sala.ClearEvents();
                }
                await uow.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error en ReservaExpirationScheduler");
            }
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
