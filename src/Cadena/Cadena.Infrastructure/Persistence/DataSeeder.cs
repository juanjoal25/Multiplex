using Cadena.Domain.Aggregates.SucursalAgg;
using Cadena.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Cadena.Infrastructure.Persistence;

public static class DataSeeder
{
    public static readonly Guid IdSucursal1 = new("00000001-0000-0000-0000-000000000001");
    public static readonly Guid IdSucursal2 = new("00000001-0000-0000-0000-000000000002");
    public static readonly Guid IdSucursal3 = new("00000001-0000-0000-0000-000000000003");
    public static readonly Guid IdSucursal4 = new("00000001-0000-0000-0000-000000000004");
    public static readonly Guid IdSucursal5 = new("00000001-0000-0000-0000-000000000005");

    private static readonly string[] NombresSucursales = [
        "Multiplex Laureles",
        "Multiplex El Tesoro",
        "Multiplex Mayorca",
        "Multiplex Envigado",
        "Multiplex Sabaneta"
    ];

    private static readonly string[] EmpresasContrato = [
        "Universidad Pontificia Bolivariana",
        "Bancolombia S.A.",
        "Grupo Éxito",
        "EPM - Empresas Públicas de Medellín",
        "Caja de Compensación Familiar Comfenalco",
        "SURA - Seguros",
        "Scotiabank Colpatria",
        "Telefónica Colombia",
        "Avianca S.A.",
        "Medellín Convention Bureau"
    ];

    private static readonly string[] CondicionesContrato = [
        "Descuento del 15% para empleados y estudiantes con carné vigente",
        "Descuento del 10% para titulares de tarjeta de crédito",
        "Descuento del 12% para afiliados con recibos de nómina",
        "Descuento del 20% para eventos corporativos mínimo 30 personas",
        "Descuento del 8% para eventos sociales y familias grandes",
        "Descuento del 15% para jubilados y pensionados",
        "Descuento del 5% para socios del programa de fidelización",
        "Acceso preferente a preventas y estrenos",
        "Combos especiales corporativos disponibles",
        "Descuento acumulable con promociones vigentes"
    ];

    public static async Task SeedAsync(CadenaDbContext db)
    {
        if (await db.Sucursales.AnyAsync()) return;

        var ahora = DateTime.UtcNow;
        var sucursales = new List<Sucursal>();
        var ids = new[] { IdSucursal1, IdSucursal2, IdSucursal3, IdSucursal4, IdSucursal5 };

        for (int i = 0; i < 5; i++)
        {
            var config = ConfiguracionGlobal.Restore(
                Guid.NewGuid(),
                "America/Bogota",
                "COP",
                [
                    ParametroGlobal.Of("precio_base_general", (15000 + i * 1000).ToString(), TipoParametro.Decimal),
                    ParametroGlobal.Of("precio_base_vip", (25000 + i * 1500).ToString(), TipoParametro.Decimal),
                    ParametroGlobal.Of("precio_base_imax", (35000 + i * 2000).ToString(), TipoParametro.Decimal),
                    ParametroGlobal.Of("minutos_expiracion_orden", "15", TipoParametro.Entero),
                    ParametroGlobal.Of("permite_reserva_online", "true", TipoParametro.Booleano),
                    ParametroGlobal.Of("capacidad_maxima_taquilla", "50", TipoParametro.Entero),
                    ParametroGlobal.Of("descuento_grupo_minimo", "10", TipoParametro.Entero),
                ]);

            var sucursal = Sucursal.Restore(
                SucursalId.Of(ids[i]),
                NombreSucursal.Of(NombresSucursales[i]),
                config,
                []);

            // Agregar 2 contratos corporativos por sucursal
            var empresas = EmpresasContrato.Skip(i * 2).Take(2);
            var daysOffset = i * 10;
            var vigencia1 = Vigencia.Of(
                ahora.AddDays(-30 - daysOffset),
                ahora.AddDays(335 - daysOffset));
            var vigencia2 = Vigencia.Of(
                ahora.AddDays(-60 - daysOffset),
                ahora.AddDays(305 - daysOffset));

            int j = 0;
            foreach (var empresa in empresas)
            {
                var vig = j == 0 ? vigencia1 : vigencia2;
                var condicion = CondicionesContrato[(i * 2 + j) % CondicionesContrato.Length];
                sucursal.RegistrarContrato(empresa, vig, condicion);
                j++;
            }

            sucursal.ClearEvents();
            sucursales.Add(sucursal);
        }

        await db.Sucursales.AddRangeAsync(sucursales);
        await db.SaveChangesAsync();
    }
}
