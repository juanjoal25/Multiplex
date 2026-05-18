using Cadena.Domain.ValueObjects;
using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Cadena.Domain.Aggregates.SucursalAgg;

public sealed class ContratoCorporativo : Entity<ContratoId>
{
    public string Tercero { get; private set; }
    public Vigencia Vigencia { get; private set; }
    public string Condiciones { get; private set; }
    public EstadoContrato Estado { get; private set; }

    private ContratoCorporativo(ContratoId id, string tercero, Vigencia v, string condiciones, EstadoContrato e) : base(id)
    {
        Tercero = tercero; Vigencia = v; Condiciones = condiciones; Estado = e;
    }

    public static ContratoCorporativo Registrar(string tercero, Vigencia vigencia, string condiciones)
    {
        if (string.IsNullOrWhiteSpace(tercero)) throw new InvariantViolationException("Tercero requerido");
        if (string.IsNullOrWhiteSpace(condiciones)) throw new InvariantViolationException("Condiciones requeridas");
        return new(ContratoId.New(), tercero.Trim(), vigencia, condiciones.Trim(), EstadoContrato.Vigente);
    }

    public static ContratoCorporativo Restore(ContratoId id, string t, Vigencia v, string c, EstadoContrato e)
        => new(id, t, v, c, e);

    public void Vencer(DateTime ahora)
    {
        if (Estado != EstadoContrato.Vigente) throw new PreconditionFailedException("Solo contrato VIGENTE puede vencer");
        if (!Vigencia.HaVencido(ahora)) throw new PreconditionFailedException("Vigencia no ha vencido aún");
        Estado = EstadoContrato.Vencido;
    }

    public void Cancelar()
    {
        if (Estado == EstadoContrato.Cancelado) throw new PreconditionFailedException("Ya cancelado");
        Estado = EstadoContrato.Cancelado;
    }
}
