using Clientes.Domain.ValueObjects;

namespace Clientes.Domain.Strategies;

public enum TipoNivel { Normal, Oro, Platino }

public interface INivel
{
    TipoNivel Tipo { get; }
    PorcentajeDescuento CalcularDescuento();
    bool PuedeAscender();
    bool PuedeDescender();
    INivel Ascender();
    INivel Descender();
}

public sealed class NivelNormal : INivel
{
    public TipoNivel Tipo => TipoNivel.Normal;
    public PorcentajeDescuento CalcularDescuento() => PorcentajeDescuento.Of(0.05m);
    public bool PuedeAscender() => true;
    public bool PuedeDescender() => false;
    public INivel Ascender() => new NivelOro();
    public INivel Descender() => throw new Shared.Kernel.Exceptions.PreconditionFailedException("NivelNormal no puede descender");
}

public sealed class NivelOro : INivel
{
    public TipoNivel Tipo => TipoNivel.Oro;
    public PorcentajeDescuento CalcularDescuento() => PorcentajeDescuento.Of(0.10m);
    public bool PuedeAscender() => true;
    public bool PuedeDescender() => true;
    public INivel Ascender() => new NivelPlatino();
    public INivel Descender() => new NivelNormal();
}

public sealed class NivelPlatino : INivel
{
    public TipoNivel Tipo => TipoNivel.Platino;
    public PorcentajeDescuento CalcularDescuento() => PorcentajeDescuento.Of(0.20m);
    public bool PuedeAscender() => false;
    public bool PuedeDescender() => true;
    public INivel Ascender() => throw new Shared.Kernel.Exceptions.PreconditionFailedException("NivelPlatino no puede ascender");
    public INivel Descender() => new NivelOro();
}

public static class NivelFactory
{
    public static INivel FromTipo(TipoNivel tipo) => tipo switch
    {
        TipoNivel.Normal => new NivelNormal(),
        TipoNivel.Oro => new NivelOro(),
        TipoNivel.Platino => new NivelPlatino(),
        _ => throw new ArgumentOutOfRangeException(nameof(tipo))
    };
}
