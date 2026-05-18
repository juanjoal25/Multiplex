using Infraestructura.Domain.Aggregates.SalaAgg;
using Infraestructura.Domain.ValueObjects;
using Shared.Kernel;

namespace Infraestructura.Domain.Repositories;

public interface ISalaRepository : IRepository<Sala, SalaId>
{
    Task<Sala?> GetBySillaIdAsync(SillaId sillaId, CancellationToken ct = default);
    Task<IReadOnlyCollection<Sala>> GetConReservasExpiradasAsync(DateTime ahora, CancellationToken ct = default);
}
