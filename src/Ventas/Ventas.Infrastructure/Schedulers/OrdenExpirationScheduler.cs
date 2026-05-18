using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ventas.Application.Abstractions;
using Ventas.Domain.Repositories;
using DomainEvents = Ventas.Domain.Events;

namespace Ventas.Infrastructure.Schedulers;

public sealed class OrdenExpirationScheduler(IServiceProvider sp, ILogger<OrdenExpirationScheduler> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = sp.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IOrdenRepository>();
                var pub = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var ordenes = await repo.GetExpiradasAsync(DateTime.UtcNow, stoppingToken);
                foreach (var o in ordenes)
                {
                    o.Expirar();
                    foreach (var e in o.DomainEvents.OfType<DomainEvents.OrdenExpirada>())
                        await pub.Publish(new global::Messaging.Contracts.Ventas.OrdenExpirada(
                            e.IdOrden.Value, e.Espectador.Value, e.Sillas, e.OccurredOn), stoppingToken);
                    o.ClearEvents();
                    await repo.UpdateAsync(o, stoppingToken);
                }
                await uow.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex) { log.LogError(ex, "Error OrdenExpirationScheduler"); }
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }
}
