using Infraestructura.Domain.Aggregates.SalaAgg;
using Infraestructura.Domain.States;
using Infraestructura.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Infrastructure.Persistence;

public static class DataSeeder
{
    // IDs de salas fijas para primeras salas
    public static readonly Guid IdSala1 = new("33333333-0000-0000-0000-000000000001"); // Sala General
    public static readonly Guid IdSala2 = new("33333333-0000-0000-0000-000000000002"); // Sala VIP
    public static readonly Guid IdSala3 = new("33333333-0000-0000-0000-000000000003"); // Sala IMAX

    // Sillas fijas para Sala General (primeras 4)
    public static readonly Guid IdSillaA1_Sal1 = new("99990001-0000-0000-0000-000000000001");
    public static readonly Guid IdSillaA2_Sal1 = new("99990001-0000-0000-0000-000000000002");
    public static readonly Guid IdSillaA3_Sal1 = new("99990001-0000-0000-0000-000000000003");
    public static readonly Guid IdSillaA4_Sal1 = new("99990001-0000-0000-0000-000000000004");

    // Sillas fijas para Sala VIP (primeras 2)
    public static readonly Guid IdSillaA1_Sal2 = new("99990002-0000-0000-0000-000000000001");
    public static readonly Guid IdSillaA2_Sal2 = new("99990002-0000-0000-0000-000000000002");

    // Sillas fijas para Sala IMAX (primeras 2)
    public static readonly Guid IdSillaA1_Sal3 = new("99990003-0000-0000-0000-000000000001");
    public static readonly Guid IdSillaA2_Sal3 = new("99990003-0000-0000-0000-000000000002");

    private static readonly (TipoSala tipo, string nombre, int aforo)[] SalasConfig = [
        (TipoSala.General, "Sala 1 General", 80),
        (TipoSala.Vip, "Sala VIP Diamante", 30),
        (TipoSala.Imax, "Sala IMAX Premier", 120),
        (TipoSala.General, "Sala 2 General", 85),
        (TipoSala.Especial, "Sala 4DX Futura", 50),
        (TipoSala.Vip, "Sala VIP Plus", 35),
        (TipoSala.General, "Sala 3 General", 90),
        (TipoSala.Imax, "Sala IMAX Gigante", 130),
        (TipoSala.General, "Sala 4 General", 80),
        (TipoSala.Especial, "Sala Infantil", 60),
        (TipoSala.Vip, "Sala VIP Gold", 40),
        (TipoSala.General, "Sala 5 General", 75),
    ];

    public static async Task SeedAsync(InfraestructuraDbContext db)
    {
        if (await db.Salas.AnyAsync()) return;

        var salas = new List<Sala>();
        // Misma convención que Programacion.DataSeeder.GetSalaId: 33333333-0000-0000-0000-{(i+1):D12}
        // Así los IDs de sala son consistentes entre ambos microservicios.
        static Guid SalaIdFijo(int i) => new($"33333333-0000-0000-0000-{(i + 1):D12}");

        for (int i = 0; i < SalasConfig.Length; i++)
        {
            var config = SalasConfig[i];
            var salaId = SalaIdFijo(i);

            var sillas = BuildSillas(i, config.tipo, config.aforo);

            var sala = Sala.Restore(
                SalaId.Of(salaId),
                config.nombre,
                config.tipo,
                Aforo.Of(config.aforo),
                EstadoSalaTipo.Disponible,
                sillas);

            salas.Add(sala);
        }

        await db.Salas.AddRangeAsync(salas);
        await db.SaveChangesAsync();
    }

    private static IEnumerable<Silla> BuildSillas(int salaIndex, TipoSala tipoSala, int aforo)
    {
        string[] filas = (aforo / 10) switch
        {
            <= 3 => ["A", "B", "C"],
            <= 8 => ["A", "B", "C", "D", "E", "F", "G", "H"],
            <= 12 => ["A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L"],
            _ => ["A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N"]
        };

        var columnasPerRow = Math.Max(8, (aforo + filas.Length - 1) / filas.Length);
        var fixedSillaIds = GetFixedSillaIds(salaIndex);

        foreach (var fila in filas)
        {
            for (var col = 1; col <= columnasPerRow && (filas.IndexOf(fila) * columnasPerRow + col) <= aforo; col++)
            {
                var key = $"{fila}{col}";
                var id = fixedSillaIds.TryGetValue(key, out var fid) ? fid : Guid.NewGuid();

                var tipoSilla = tipoSala switch
                {
                    TipoSala.Vip => TipoSilla.Vip,
                    TipoSala.Especial => col <= 2 ? TipoSilla.Especial : TipoSilla.General,
                    _ => TipoSilla.General
                };

                yield return Silla.Restore(
                    SillaId.Of(id),
                    Posicion.Of(fila, col),
                    tipoSilla,
                    EstadoSillaTipo.Disponible,
                    null, null, null);
            }
        }
    }

    private static Dictionary<string, Guid> GetFixedSillaIds(int salaIndex) => salaIndex switch
    {
        0 => new()
        {
            ["A1"] = IdSillaA1_Sal1,
            ["A2"] = IdSillaA2_Sal1,
            ["A3"] = IdSillaA3_Sal1,
            ["A4"] = IdSillaA4_Sal1,
        },
        1 => new()
        {
            ["A1"] = IdSillaA1_Sal2,
            ["A2"] = IdSillaA2_Sal2,
        },
        2 => new()
        {
            ["A1"] = IdSillaA1_Sal3,
            ["A2"] = IdSillaA2_Sal3,
        },
        _ => []
    };
}
