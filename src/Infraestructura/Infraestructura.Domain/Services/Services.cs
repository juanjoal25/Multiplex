using Infraestructura.Domain.Aggregates.SalaAgg;
using Infraestructura.Domain.States;
using Infraestructura.Domain.ValueObjects;

namespace Infraestructura.Domain.Services;

public sealed class SvcGestionAforo
{
    public int CalcularOcupacion(Sala sala) =>
        sala.Sillas.Count(s => s.Estado.Tipo is EstadoSillaTipo.Reservada or EstadoSillaTipo.Ocupada);

    public int CalcularDisponibles(Sala sala) =>
        sala.Sillas.Count(s => s.Estado.Tipo == EstadoSillaTipo.Disponible);

    public bool VerificarAforo(Sala sala) => CalcularDisponibles(sala) > 0;
}

public sealed class SvcLiberacionReservas
{
    public IReadOnlyCollection<SillaId> LiberarReservasExpiradas(IEnumerable<Sala> salas, DateTime ahora)
    {
        var all = new List<SillaId>();
        foreach (var sala in salas)
        {
            var liberadas = sala.LiberarExpiradas(ahora);
            all.AddRange(liberadas);
        }
        return all;
    }
}

public sealed class SvcCambioEstadoSala
{
    public void EnviarMantenimiento(Sala sala) => sala.EnviarMantenimiento();
    public void Reactivar(Sala sala) => sala.Reactivar();
}
