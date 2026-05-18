using Programacion.Domain.Aggregates.AlquilerAgg;
using Programacion.Domain.Aggregates.CarteleraAgg;
using Programacion.Domain.Aggregates.FuncionAgg;
using Programacion.Domain.Aggregates.PeliculaAgg;
using Programacion.Domain.ValueObjects;
using Shared.Kernel;

namespace Programacion.Domain.Repositories;

public interface IPeliculaRepository : IRepository<Pelicula, PeliculaId> { }

public interface IFuncionRepository : IRepository<Funcion, FuncionId>
{
    Task<IReadOnlyCollection<Funcion>> GetByVigentesEnSalaAsync(Guid idSala, CancellationToken ct = default);
}

public interface ICarteleraRepository : IRepository<Cartelera, CarteleraId>
{
    Task<Cartelera?> GetVigenteAsync(DateTime ahora, CancellationToken ct = default);
}

public interface IAlquilerRepository : IRepository<Alquiler, AlquilerId> { }
