using Cadena.Domain.Aggregates.SucursalAgg;
using Cadena.Domain.ValueObjects;
using Shared.Kernel;

namespace Cadena.Domain.Repositories;

public interface ISucursalRepository : IRepository<Sucursal, SucursalId>
{
    Task<IReadOnlyCollection<Sucursal>> GetTodasAsync(CancellationToken ct = default);
    Task<Sucursal?> GetByContratoIdAsync(ContratoId id, CancellationToken ct = default);
}
