using Programacion.Domain.Events;
using Programacion.Domain.Strategies;
using Programacion.Domain.ValueObjects;
using Shared.Kernel;

namespace Programacion.Domain.Aggregates.PeliculaAgg;

public sealed class Pelicula : AggregateRoot<PeliculaId>
{
    public Titulo Titulo { get; private set; }
    public Clasificacion Clasificacion { get; private set; }
    public Genero Genero { get; private set; }
    public Duracion Duracion { get; private set; }
    public TipoFormato FormatoDisponible { get; private set; }

    private Pelicula(PeliculaId id, Titulo titulo, Clasificacion clas, Genero genero, Duracion duracion, TipoFormato formato)
        : base(id)
    {
        Titulo = titulo; Clasificacion = clas; Genero = genero; Duracion = duracion; FormatoDisponible = formato;
    }

    public static Pelicula Registrar(Titulo titulo, Clasificacion clas, Genero genero, Duracion duracion, TipoFormato formato)
    {
        var id = PeliculaId.New();
        var p = new Pelicula(id, titulo, clas, genero, duracion, formato);
        p.Raise(new PeliculaRegistrada(id, titulo.Valor, clas, genero.Valor, duracion.Minutos));
        return p;
    }

    public static Pelicula Restore(PeliculaId id, Titulo titulo, Clasificacion clas, Genero genero, Duracion duracion, TipoFormato formato)
        => new(id, titulo, clas, genero, duracion, formato);
}
