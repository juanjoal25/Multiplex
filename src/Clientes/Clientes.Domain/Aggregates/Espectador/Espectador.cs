using Clientes.Domain.Events;
using Clientes.Domain.States;
using Clientes.Domain.Strategies;
using Clientes.Domain.ValueObjects;
using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Clientes.Domain.Aggregates.EspectadorAgg;

public sealed class Espectador : AggregateRoot<EspectadorId>
{
    public NombreCompleto Nombre { get; private set; }
    public Email Correo { get; private set; }
    public Documento Documento { get; private set; }
    public Suscripcion Suscripcion { get; private set; }

    private Espectador() { Nombre = null!; Correo = null!; Documento = null!; Suscripcion = null!; }

    private Espectador(EspectadorId id, NombreCompleto nombre, Email correo, Documento documento, Suscripcion suscripcion)
        : base(id)
    {
        Nombre = nombre;
        Correo = correo;
        Documento = documento;
        Suscripcion = suscripcion;
    }

    public static Espectador Registrar(NombreCompleto nombre, Email correo, Documento documento)
    {
        ArgumentNullException.ThrowIfNull(nombre);
        ArgumentNullException.ThrowIfNull(correo);
        ArgumentNullException.ThrowIfNull(documento);

        var id = EspectadorId.New();
        var sus = Suscripcion.Inicial();
        var e = new Espectador(id, nombre, correo, documento, sus);
        e.Raise(new EspectadorRegistrado(id, sus.Nivel.Tipo));
        return e;
    }

    public static Espectador Restore(EspectadorId id, NombreCompleto nombre, Email correo, Documento documento, Suscripcion sus)
        => new(id, nombre, correo, documento, sus);

    public PorcentajeDescuento PorcentajeDescuento() => Suscripcion.PorcentajeDescuento();

    public void ActivarMembresia(Vigencia vigencia)
    {
        Suscripcion.Activar(vigencia);
        Raise(new MembresiaActivada(Id, Suscripcion.Nivel.Tipo, vigencia.FechaInicio, vigencia.FechaFin));
    }

    public void Ascender()
    {
        var (anterior, nuevo) = Suscripcion.Ascender();
        Raise(new NivelAscendido(Id, anterior, nuevo));
    }

    public void Descender()
    {
        var (anterior, nuevo) = Suscripcion.Descender();
        Raise(new NivelDescendido(Id, anterior, nuevo));
    }

    public void ExpirarMembresia(DateTime ahora)
    {
        var anterior = Suscripcion.Expirar(ahora);
        Raise(new MembresiaExpirada(Id, anterior));
    }

    public void CancelarMembresia(string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new PreconditionFailedException("Motivo no puede ser vacío");
        Suscripcion.Cancelar();
        Raise(new MembresiaCancelada(Id, motivo));
    }

    public void ReactivarMembresia(Vigencia nuevaVigencia)
    {
        Suscripcion.Reactivar(nuevaVigencia);
        Raise(new MembresiaActivada(Id, Suscripcion.Nivel.Tipo, nuevaVigencia.FechaInicio, nuevaVigencia.FechaFin));
    }
}
