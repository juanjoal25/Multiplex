using Financiero.Domain.Aggregates.TransaccionAgg;
using Financiero.Domain.ValueObjects;
using Shared.Kernel;

namespace Financiero.Domain.Repositories;

public interface ITransaccionRepository : IRepository<Transaccion, TransaccionId>
{
    Task<Transaccion?> GetByOrdenIdAsync(Guid idOrden, CancellationToken ct = default);
    Task<bool> ExistsByOrdenIdAsync(Guid idOrden, CancellationToken ct = default);
    Task<IReadOnlyCollection<Transaccion>> GetHistorialAsync(DateTime inicio, DateTime fin, CancellationToken ct = default);
}
