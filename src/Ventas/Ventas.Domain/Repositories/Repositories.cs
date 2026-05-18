using Shared.Kernel;
using Ventas.Domain.Aggregates.DefComboAgg;
using Ventas.Domain.Aggregates.OrdenAgg;
using Ventas.Domain.Aggregates.ProductoAgg;
using Ventas.Domain.ValueObjects;

namespace Ventas.Domain.Repositories;

public interface IOrdenRepository : IRepository<Orden, OrdenId>
{
    Task<IReadOnlyCollection<Orden>> GetExpiradasAsync(DateTime ahora, CancellationToken ct = default);
    Task<IReadOnlyCollection<Orden>> GetPendientesConFuncionAsync(Guid idFuncion, CancellationToken ct = default);
}

public interface IProductoRepository : IRepository<ProductoConfiteria, ProductoId> { }
public interface IDefComboRepository : IRepository<DefCombo, DefComboId> { }
