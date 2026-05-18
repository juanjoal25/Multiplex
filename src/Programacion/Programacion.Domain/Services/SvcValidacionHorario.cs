using Programacion.Domain.Aggregates.FuncionAgg;
using Programacion.Domain.ValueObjects;

namespace Programacion.Domain.Services;

public sealed class SvcValidacionHorario
{
    public bool ValidarDisponibilidadSala(SalaRef sala, RangoHorario propuesto, IEnumerable<Funcion> funcionesExistentes)
    {
        ArgumentNullException.ThrowIfNull(sala);
        ArgumentNullException.ThrowIfNull(propuesto);
        ArgumentNullException.ThrowIfNull(funcionesExistentes);

        return !funcionesExistentes.Any(f =>
            f.SalaRef.IdSala == sala.IdSala && f.Horario.SolapaCon(propuesto));
    }

    public IEnumerable<Funcion> ObtenerConflictos(SalaRef sala, RangoHorario propuesto, IEnumerable<Funcion> funcionesExistentes)
        => funcionesExistentes.Where(f => f.SalaRef.IdSala == sala.IdSala && f.Horario.SolapaCon(propuesto));
}
