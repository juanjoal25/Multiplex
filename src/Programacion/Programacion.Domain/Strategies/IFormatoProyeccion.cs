using Programacion.Domain.ValueObjects;
using Shared.Kernel.ValueObjects;

namespace Programacion.Domain.Strategies;

public enum TipoFormato { Formato2D, Formato3D, FormatoIMAX, Formato4DX }

public interface IFormatoProyeccion
{
    TipoFormato Tipo { get; }
    bool EsCompatibleConSala(TipoSala tipoSala);
    Money PrecioExtra();
}

public sealed class Formato2D : IFormatoProyeccion
{
    public TipoFormato Tipo => TipoFormato.Formato2D;
    public bool EsCompatibleConSala(TipoSala t) => t is TipoSala.General or TipoSala.Vip;
    public Money PrecioExtra() => Money.Zero();
}

public sealed class Formato3D : IFormatoProyeccion
{
    public TipoFormato Tipo => TipoFormato.Formato3D;
    public bool EsCompatibleConSala(TipoSala t) => t is TipoSala.General or TipoSala.Vip or TipoSala.Imax;
    public Money PrecioExtra() => Money.Of(5000m);
}

public sealed class FormatoIMAX : IFormatoProyeccion
{
    public TipoFormato Tipo => TipoFormato.FormatoIMAX;
    public bool EsCompatibleConSala(TipoSala t) => t is TipoSala.Imax;
    public Money PrecioExtra() => Money.Of(10000m);
}

public sealed class Formato4DX : IFormatoProyeccion
{
    public TipoFormato Tipo => TipoFormato.Formato4DX;
    public bool EsCompatibleConSala(TipoSala t) => t is TipoSala.Especial;
    public Money PrecioExtra() => Money.Of(15000m);
}

public static class FormatoFactory
{
    public static IFormatoProyeccion FromTipo(TipoFormato tipo) => tipo switch
    {
        TipoFormato.Formato2D => new Formato2D(),
        TipoFormato.Formato3D => new Formato3D(),
        TipoFormato.FormatoIMAX => new FormatoIMAX(),
        TipoFormato.Formato4DX => new Formato4DX(),
        _ => throw new ArgumentOutOfRangeException(nameof(tipo))
    };
}
