using Cadena.Domain.ValueObjects;
using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Cadena.Domain.Aggregates.SucursalAgg;

public sealed class ConfiguracionGlobal : Entity<Guid>
{
    private readonly Dictionary<string, ParametroGlobal> _parametros = new();

    public string ZonaHoraria { get; private set; }
    public string Moneda { get; private set; }
    public IReadOnlyCollection<ParametroGlobal> Parametros => _parametros.Values;

    private ConfiguracionGlobal(Guid id, string zh, string moneda, IEnumerable<ParametroGlobal>? p = null) : base(id)
    {
        ZonaHoraria = zh; Moneda = moneda;
        if (p is not null) foreach (var x in p) _parametros[x.Clave] = x;
    }

    public static ConfiguracionGlobal Crear(string zonaHoraria, string moneda)
    {
        if (string.IsNullOrWhiteSpace(zonaHoraria)) throw new InvariantViolationException("ZonaHoraria requerida");
        if (string.IsNullOrWhiteSpace(moneda)) throw new InvariantViolationException("Moneda requerida");
        return new(Guid.NewGuid(), zonaHoraria.Trim(), moneda.Trim().ToUpperInvariant());
    }

    public static ConfiguracionGlobal Restore(Guid id, string zh, string moneda, IEnumerable<ParametroGlobal> p)
        => new(id, zh, moneda, p);

    public IReadOnlyCollection<ParametroGlobal> Actualizar(IEnumerable<ParametroGlobal> nuevos)
    {
        var mods = new List<ParametroGlobal>();
        foreach (var p in nuevos)
        {
            _parametros[p.Clave] = p;
            mods.Add(p);
        }
        return mods;
    }

    public ParametroGlobal? Obtener(string clave) => _parametros.TryGetValue(clave, out var p) ? p : null;
}
