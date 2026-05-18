using Clientes.Domain.Aggregates.EspectadorAgg;
using Clientes.Domain.ValueObjects;
using Shared.Kernel;

namespace Clientes.Domain.Repositories;

public interface IEspectadorRepository : IRepository<Espectador, EspectadorId>
{
    Task<Espectador?> GetByDocumentoAsync(Documento documento, CancellationToken ct = default);
    Task<bool> ExistsByDocumentoAsync(Documento documento, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(Email email, CancellationToken ct = default);
}
