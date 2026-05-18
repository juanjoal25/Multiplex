using Cadena.Domain.Aggregates.SucursalAgg;
using Cadena.Domain.ValueObjects;

namespace Cadena.Domain.Services;

public sealed class SvcPropagacionConfiguracion
{
    public IReadOnlyCollection<ParametroGlobal> Recolectar(Sucursal sucursal, IEnumerable<string>? clavesFiltro = null)
    {
        if (clavesFiltro is null) return sucursal.Configuracion.Parametros.ToList();
        var set = clavesFiltro.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return sucursal.Configuracion.Parametros.Where(p => set.Contains(p.Clave)).ToList();
    }
}

public sealed class SvcGestionContratos
{
    public IReadOnlyCollection<ContratoCorporativo> VerificarVigencias(IEnumerable<Sucursal> sucursales, DateTime ahora)
    {
        var vencidos = new List<ContratoCorporativo>();
        foreach (var s in sucursales)
            vencidos.AddRange(s.VencerContratos(ahora));
        return vencidos;
    }

    public IReadOnlyCollection<ContratoCorporativo> ConsultarVigentes(Sucursal sucursal, string tercero, DateTime ahora)
    {
        return sucursal.Contratos
            .Where(c => c.Estado == EstadoContrato.Vigente
                && string.Equals(c.Tercero, tercero, StringComparison.OrdinalIgnoreCase)
                && c.Vigencia.EstaVigenteEn(ahora))
            .ToList();
    }
}
