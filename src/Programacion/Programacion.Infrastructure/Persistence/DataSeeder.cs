using Microsoft.EntityFrameworkCore;
using Programacion.Domain.Aggregates.CarteleraAgg;
using Programacion.Domain.Aggregates.FuncionAgg;
using Programacion.Domain.Aggregates.PeliculaAgg;
using Programacion.Domain.States;
using Programacion.Domain.Strategies;
using Programacion.Domain.ValueObjects;

namespace Programacion.Infrastructure.Persistence;

public static class DataSeeder
{
    // IDs fijos para primeras películas y funciones
    public static readonly Guid IdPel1 = new("22222222-0000-0000-0000-000000000001");
    public static readonly Guid IdPel2 = new("22222222-0000-0000-0000-000000000002");
    public static readonly Guid IdPel3 = new("22222222-0000-0000-0000-000000000003");
    public static readonly Guid IdPel4 = new("22222222-0000-0000-0000-000000000004");
    public static readonly Guid IdPel5 = new("22222222-0000-0000-0000-000000000005");

    public static readonly Guid IdFun1 = new("44444444-0000-0000-0000-000000000001");
    public static readonly Guid IdFun2 = new("44444444-0000-0000-0000-000000000002");
    public static readonly Guid IdFun3 = new("44444444-0000-0000-0000-000000000003");
    public static readonly Guid IdFun4 = new("44444444-0000-0000-0000-000000000004");

    private static readonly (string titulo, Clasificacion clasificacion, string genero, int duracion)[] Peliculas = [
        ("Spider-Man: Multiverso Final", Clasificacion.PG13, "Acción/Aventura", 135),
        ("Avatar: El Último Cielo", Clasificacion.PG13, "Ciencia Ficción", 182),
        ("La Mansión Maldita", Clasificacion.R, "Terror", 98),
        ("Shrek 5: Felices Para Siempre", Clasificacion.G, "Animación/Comedia", 108),
        ("Origen 2: El Sueño Profundo", Clasificacion.PG13, "Thriller/Ciencia Ficción", 148),
        ("Dune: Profecía del Desierto", Clasificacion.PG13, "Ciencia Ficción/Drama", 166),
        ("Barbie 2: La Aventura Continúa", Clasificacion.G, "Comedia/Aventura", 112),
        ("El Exorcista Resurrección", Clasificacion.R, "Terror Sobrenatural", 134),
        ("Top Gun: Maverick 2", Clasificacion.PG13, "Acción/Drama", 152),
        ("Frozen 3: Reino Eterno", Clasificacion.G, "Animación/Musical", 118),
        ("Gladiador 3: Legado Imperial", Clasificacion.R, "Acción/Historia", 168),
        ("Inside Out 3: Nuevas Emociones", Clasificacion.G, "Animación/Comedia", 105),
        ("Oppenheimer 2: Secretos del Átomo", Clasificacion.R, "Drama/Historia", 175),
        ("Wonka 2: Más Magia", Clasificacion.G, "Fantasía/Comedia", 128),
        ("Kingdom of the Planet of the Apes 2", Clasificacion.PG13, "Ciencia Ficción/Drama", 145),
        ("The Brutalist", Clasificacion.R, "Drama Épico", 188),
        ("Sonic 4: La Carrera Final", Clasificacion.G, "Acción/Comedia", 98),
        ("Aquaman y el Trono Perdido", Clasificacion.PG13, "Acción/Aventura", 158),
        ("Paddington en América", Clasificacion.G, "Aventura/Comedia", 110),
        ("The Brutalist Extended Cut", Clasificacion.R, "Drama", 210),
    ];

    private static readonly TipoFormato[] Formatos = [
        TipoFormato.Formato2D,
        TipoFormato.Formato3D,
        TipoFormato.FormatoIMAX,
        TipoFormato.Formato4DX
    ];

    private static readonly int[] HorariosPrincipioPelicula = [11, 14, 17, 20]; // Matinée, tarde, noche, tarde-noche

    public static async Task SeedAsync(ProgramacionDbContext db)
    {
        if (await db.Carteleras.AnyAsync()) return;

        if (await db.Peliculas.AnyAsync())
        {
            db.Peliculas.RemoveRange(db.Peliculas);
            db.Funciones.RemoveRange(db.Funciones);
            await db.SaveChangesAsync();
        }

        var ahora = DateTime.UtcNow;
        var manana = ahora.Date.AddDays(1);
        var random = new Random(42);

        // ─── Crear 20 películas ───
        var peliculasIds = new List<Guid>();
        var peliculasFixas = new Dictionary<int, Guid>
        {
            [0] = IdPel1,
            [1] = IdPel2,
            [2] = IdPel3,
            [3] = IdPel4,
            [4] = IdPel5,
        };

        for (int i = 0; i < Peliculas.Length; i++)
        {
            var pel = Peliculas[i];
            var id = peliculasFixas.TryGetValue(i, out var fid) ? fid : Guid.NewGuid();
            peliculasIds.Add(id);

            var formato = pel.titulo.Contains("IMAX") || pel.titulo.Contains("4DX")
                ? (pel.titulo.Contains("4DX") ? TipoFormato.Formato4DX : TipoFormato.FormatoIMAX)
                : (random.Next(100) < 30 ? TipoFormato.Formato3D : TipoFormato.Formato2D);

            var pelicula = Pelicula.Restore(
                PeliculaId.Of(id),
                Titulo.Of(pel.titulo),
                pel.clasificacion,
                Genero.Of(pel.genero),
                Duracion.Of(pel.duracion),
                formato);

            await db.Peliculas.AddAsync(pelicula);
        }

        await db.SaveChangesAsync();

        // ─── Crear múltiples funciones por película ───
        var funciones = new List<Funcion>();
        var funcionesIds = new List<Guid>();
        var funcionesFixas = new Dictionary<int, Guid>
        {
            [0] = IdFun1,
            [1] = IdFun2,
            [2] = IdFun3,
            [3] = IdFun4,
        };

        int funcionIndex = 0;
        for (int dia = 0; dia < 7; dia++) // Programar funciones para los próximos 7 días
        {
            var diaActual = manana.AddDays(dia);

            for (int peliculaIdx = 0; peliculaIdx < peliculasIds.Count; peliculaIdx++)
            {
                // 2-3 funciones por película por día (en salas diferentes)
                int funcionesPorPelicula = random.Next(2, 4);
                for (int func = 0; func < funcionesPorPelicula && funcionIndex < 80; func++)
                {
                    var salaIdx = (peliculaIdx + func) % 12; // Distribuir entre 12 salas
                    var salaId = GetSalaId(salaIdx);
                    var tipoSala = GetTipoSala(salaIdx);

                    var horario = HorariosPrincipioPelicula[func % HorariosPrincipioPelicula.Length];
                    var inicio = diaActual.AddHours(horario);

                    var pelId = PeliculaId.Of(peliculasIds[peliculaIdx]);
                    var pelicula = await db.Peliculas.FirstAsync(p => p.Id == pelId);
                    var duracionMin = pelicula.Duracion.Minutos;

                    var fin = inicio.AddMinutes(duracionMin + 10); // +10 min limpieza

                    var formato = random.Next(100) < 20
                        ? TipoFormato.FormatoIMAX
                        : (random.Next(100) < 40 ? TipoFormato.Formato3D : TipoFormato.Formato2D);

                    var id = funcionesFixas.TryGetValue(funcionIndex, out var fid) ? fid : Guid.NewGuid();
                    funcionesIds.Add(id);

                    var funcion = Funcion.Restore(
                        FuncionId.Of(id),
                        PeliculaRef.Of(peliculasIds[peliculaIdx]),
                        SalaRef.Of(salaId, tipoSala),
                        RangoHorario.Of(inicio, fin),
                        formato,
                        EstadoFuncionTipo.Programada);

                    funciones.Add(funcion);
                    funcionIndex++;
                }
            }
        }

        // Guardar funciones en lotes
        const int BatchSize = 25;
        for (int i = 0; i < funciones.Count; i += BatchSize)
        {
            await db.Funciones.AddRangeAsync(funciones.Skip(i).Take(BatchSize));
            await db.SaveChangesAsync();
        }

        // ─── Crear cartelera vigente (desde hoy, 2 semanas) ───
        var carteleraInicio = ahora.Date;
        var carteleraFin = carteleraInicio.AddDays(15);

        var cartelera = Cartelera.Restore(
            CarteleraId.New(),
            PeriodoCartelera.Of(carteleraInicio, carteleraFin),
            funcionesIds.Take(Math.Min(50, funcionesIds.Count))); // Primeras 50 funciones en cartelera

        cartelera.ClearEvents();
        await db.Carteleras.AddAsync(cartelera);
        await db.SaveChangesAsync();
    }

    private static Guid GetSalaId(int index) => index switch
    {
        0 => new("33333333-0000-0000-0000-000000000001"),
        1 => new("33333333-0000-0000-0000-000000000002"),
        2 => new("33333333-0000-0000-0000-000000000003"),
        _ => new Guid($"33333333-0000-0000-0000-{(index + 1):D12}")
    };

    private static TipoSala GetTipoSala(int index) => index switch
    {
        0 => TipoSala.General,
        1 => TipoSala.Vip,
        2 => TipoSala.Imax,
        3 => TipoSala.General,
        4 => TipoSala.Especial,
        5 => TipoSala.Vip,
        6 => TipoSala.General,
        7 => TipoSala.Imax,
        8 => TipoSala.General,
        9 => TipoSala.Especial,
        10 => TipoSala.Vip,
        _ => TipoSala.General
    };
}
