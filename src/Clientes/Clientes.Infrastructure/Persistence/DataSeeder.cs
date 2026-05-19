using Clientes.Domain.Aggregates.EspectadorAgg;
using Clientes.Domain.States;
using Clientes.Domain.Strategies;
using Clientes.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Clientes.Infrastructure.Persistence;

public static class DataSeeder
{
    // IDs fijos para clientes que usan en órdenes
    public static readonly Guid IdEsp1 = new("11111111-0000-0000-0000-000000000001");
    public static readonly Guid IdEsp2 = new("11111111-0000-0000-0000-000000000002");
    public static readonly Guid IdEsp3 = new("11111111-0000-0000-0000-000000000003");
    public static readonly Guid IdEsp4 = new("11111111-0000-0000-0000-000000000004");
    public static readonly Guid IdEsp5 = new("11111111-0000-0000-0000-000000000005");

    private static readonly string[] Nombres = [
        "Ana", "Carlos", "María", "Juan", "Sofia", "Diego", "Laura", "Roberto", "Valentina", "Miguel",
        "Paula", "Felipe", "Camila", "Andrés", "Daniela", "Ricardo", "Alejandra", "Fernando", "Gabriela", "Luis",
        "Carolina", "Javier", "Martina", "Eduardo", "Isabella", "Raúl", "Natalia", "Sergio", "Paola", "Víctor",
        "Fernanda", "Mauricio", "Jimena", "Arturo", "Rocío", "Francisco", "Lucía", "Guillermo", "Florencia", "Ramón"
    ];

    private static readonly string[] Apellidos = [
        "Torres", "Ruiz", "López", "Gómez", "Vargas", "Martínez", "García", "Rodríguez", "Moreno", "Sánchez",
        "Pérez", "González", "Ramírez", "Hernández", "Castro", "Flores", "Rivera", "Bravo", "Salazar", "Vega",
        "Medina", "Navarro", "Cortés", "Domínguez", "Espinoza", "Fuentes", "Guerrero", "Herrera", "Jiménez", "Latorre",
        "Mejía", "Miranda", "Molina", "Montoya", "Morales", "Ochoa", "Ortega", "Pacheco", "Padilla", "Parra"
    ];

    private static readonly string[] Dominios = [
        "gmail.com", "hotmail.com", "yahoo.com", "outlook.com", "live.com", "corporativo.com"
    ];

    private static readonly TipoDocumento[] TiposDocumento = [TipoDocumento.CC, TipoDocumento.CE, TipoDocumento.PAS];

    public static async Task SeedAsync(ClientesDbContext db)
    {
        if (await db.Espectadores.AnyAsync()) return;

        var ahora = DateTime.UtcNow;
        var espectadores = new List<Espectador>();
        var random = new Random(42); // Seed para reproducibilidad

        // ─── Primeros 5 clientes con IDs fijos (usados en órdenes) ───
        var vigenciaOro = Vigencia.Of(ahora.AddDays(-60), ahora.AddDays(305));
        var vigenciaPlatino = Vigencia.Of(ahora.AddDays(-30), ahora.AddDays(335));
        var vigenciaOro2 = Vigencia.Of(ahora.AddDays(-15), ahora.AddDays(350));

        espectadores.AddRange(new[]
        {
            Espectador.Restore(
                EspectadorId.Of(IdEsp1),
                NombreCompleto.Of("Ana", "Torres"),
                Email.Of("ana.torres@multiplex.co"),
                Documento.Of(TipoDocumento.CC, "1001234567"),
                Suscripcion.Restore(Guid.NewGuid(), EstadoSuscripcionTipo.Activa, TipoNivel.Normal, null)),

            Espectador.Restore(
                EspectadorId.Of(IdEsp2),
                NombreCompleto.Of("Carlos", "Ruiz"),
                Email.Of("carlos.ruiz@multiplex.co"),
                Documento.Of(TipoDocumento.CC, "1007654321"),
                Suscripcion.Restore(Guid.NewGuid(), EstadoSuscripcionTipo.Activa, TipoNivel.Oro, vigenciaOro)),

            Espectador.Restore(
                EspectadorId.Of(IdEsp3),
                NombreCompleto.Of("María", "López"),
                Email.Of("maria.lopez@multiplex.co"),
                Documento.Of(TipoDocumento.CE, "CE987654"),
                Suscripcion.Restore(Guid.NewGuid(), EstadoSuscripcionTipo.Activa, TipoNivel.Platino, vigenciaPlatino)),

            Espectador.Restore(
                EspectadorId.Of(IdEsp4),
                NombreCompleto.Of("Juan", "Gómez"),
                Email.Of("juan.gomez@multiplex.co"),
                Documento.Of(TipoDocumento.CC, "1098765432"),
                Suscripcion.Restore(Guid.NewGuid(), EstadoSuscripcionTipo.Activa, TipoNivel.Normal, null)),

            Espectador.Restore(
                EspectadorId.Of(IdEsp5),
                NombreCompleto.Of("Sofia", "Vargas"),
                Email.Of("sofia.vargas@multiplex.co"),
                Documento.Of(TipoDocumento.PAS, "PAS2024COL"),
                Suscripcion.Restore(Guid.NewGuid(), EstadoSuscripcionTipo.Activa, TipoNivel.Oro, vigenciaOro2)),
        });

        // ─── 145 clientes adicionales generados ───
        for (int i = 0; i < 145; i++)
        {
            var nombre = Nombres[random.Next(Nombres.Length)];
            var apellido = Apellidos[random.Next(Apellidos.Length)];
            var dominio = Dominios[random.Next(Dominios.Length)];
            var email = $"{nombre.ToLower()}.{apellido.ToLower()}{i}@{dominio}";

            var tipoDocumento = TiposDocumento[random.Next(TiposDocumento.Length)];
            var numeroDoc = tipoDocumento switch
            {
                TipoDocumento.CC => (1000000000 + i).ToString(),
                TipoDocumento.CE => $"CE{100000 + i}",
                TipoDocumento.PAS => $"PAS{2025 + (i / 10)}{i % 10:D3}COL",
                _ => throw new InvalidOperationException()
            };

            // Distribución de niveles: 60% Normal, 30% Oro, 10% Platino
            var nivelRandom = random.NextDouble();
            TipoNivel nivel;
            Vigencia? vigencia = null;

            if (nivelRandom < 0.60)
            {
                nivel = TipoNivel.Normal;
            }
            else if (nivelRandom < 0.90)
            {
                nivel = TipoNivel.Oro;
                var daysOffset = random.Next(-60, 60);
                vigencia = Vigencia.Of(ahora.AddDays(daysOffset - 30), ahora.AddDays(daysOffset + 335));
            }
            else
            {
                nivel = TipoNivel.Platino;
                var daysOffset = random.Next(-30, 30);
                vigencia = Vigencia.Of(ahora.AddDays(daysOffset - 15), ahora.AddDays(daysOffset + 350));
            }

            var espectador = Espectador.Restore(
                EspectadorId.New(),
                NombreCompleto.Of(nombre, apellido),
                Email.Of(email),
                Documento.Of(tipoDocumento, numeroDoc),
                Suscripcion.Restore(Guid.NewGuid(), EstadoSuscripcionTipo.Activa, nivel, vigencia));

            espectadores.Add(espectador);
        }

        // Guardar en lotes para mejor performance
        const int BatchSize = 50;
        for (int i = 0; i < espectadores.Count; i += BatchSize)
        {
            await db.Espectadores.AddRangeAsync(espectadores.Skip(i).Take(BatchSize));
            await db.SaveChangesAsync();
        }
    }
}
