using Cadena.Domain.Events;
using Cadena.Domain.ValueObjects;
using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Cadena.Domain.Aggregates.SucursalAgg;

public sealed class Sucursal : AggregateRoot<SucursalId>
{
    private readonly List<ContratoCorporativo> _contratos = new();

    public NombreSucursal Nombre { get; private set; }
    public ConfiguracionGlobal Configuracion { get; private set; }
    public IReadOnlyCollection<ContratoCorporativo> Contratos => _contratos.AsReadOnly();

    private Sucursal() { Nombre = null!; Configuracion = null!; }

    private Sucursal(SucursalId id, NombreSucursal nombre, ConfiguracionGlobal config, IEnumerable<ContratoCorporativo>? contratos = null) : base(id)
    {
        Nombre = nombre; Configuracion = config;
        if (contratos is not null) _contratos.AddRange(contratos);
    }

    public static Sucursal Crear(NombreSucursal nombre, ConfiguracionGlobal config)
        => new(SucursalId.New(), nombre, config);

    public static Sucursal Restore(SucursalId id, NombreSucursal nombre, ConfiguracionGlobal config, IEnumerable<ContratoCorporativo> contratos)
        => new(id, nombre, config, contratos);

    public void ActualizarConfiguracion(IEnumerable<ParametroGlobal> nuevos)
    {
        var lista = nuevos.ToList();
        if (lista.Count == 0) throw new PreconditionFailedException("ParametrosModificados no puede estar vacío");
        var mods = Configuracion.Actualizar(lista);
        Raise(new ConfiguracionActualizada(Id, mods));
    }

    public ContratoCorporativo RegistrarContrato(string tercero, Vigencia vigencia, string condiciones)
    {
        // Invariante: no dos contratos VIGENTES con mismo tercero en rangos solapados
        var conflicto = _contratos.Any(c =>
            c.Estado == EstadoContrato.Vigente &&
            string.Equals(c.Tercero, tercero, StringComparison.OrdinalIgnoreCase) &&
            c.Vigencia.FechaInicio < vigencia.FechaFin &&
            vigencia.FechaInicio < c.Vigencia.FechaFin);
        if (conflicto)
            throw new InvariantViolationException("Ya existe un contrato VIGENTE con ese tercero en rango solapado");

        var contrato = ContratoCorporativo.Registrar(tercero, vigencia, condiciones);
        _contratos.Add(contrato);
        Raise(new ContratoCorporativoRegistrado(contrato.Id, Id, tercero, vigencia));
        return contrato;
    }

    public void CancelarContrato(ContratoId idContrato, string motivo)
    {
        var c = _contratos.FirstOrDefault(x => x.Id == idContrato)
            ?? throw new PreconditionFailedException("Contrato no existe");
        if (string.IsNullOrWhiteSpace(motivo)) throw new PreconditionFailedException("Motivo requerido");
        c.Cancelar();
        Raise(new ContratoCorporativoCancelado(c.Id, Id, motivo));
    }

    public IReadOnlyCollection<ContratoCorporativo> VencerContratos(DateTime ahora)
    {
        var vencidos = new List<ContratoCorporativo>();
        foreach (var c in _contratos.Where(x => x.Estado == EstadoContrato.Vigente && x.Vigencia.HaVencido(ahora)))
        {
            c.Vencer(ahora);
            vencidos.Add(c);
            Raise(new ContratoCorporativoVencido(c.Id, Id));
        }
        return vencidos;
    }

    public bool TieneContratoVigente(string tercero, DateTime ahora) =>
        _contratos.Any(c => c.Estado == EstadoContrato.Vigente &&
            string.Equals(c.Tercero, tercero, StringComparison.OrdinalIgnoreCase) &&
            c.Vigencia.EstaVigenteEn(ahora));
}
