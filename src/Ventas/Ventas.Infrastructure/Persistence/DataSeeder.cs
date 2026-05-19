using Microsoft.EntityFrameworkCore;
using Shared.Kernel.ValueObjects;
using Ventas.Domain.Aggregates.DefComboAgg;
using Ventas.Domain.Aggregates.OrdenAgg;
using Ventas.Domain.Aggregates.ProductoAgg;
using Ventas.Domain.ValueObjects;

namespace Ventas.Infrastructure.Persistence;

public static class DataSeeder
{
    // IDs de espectadores fijos
    private static readonly Guid IdEsp1 = new("11111111-0000-0000-0000-000000000001");
    private static readonly Guid IdEsp2 = new("11111111-0000-0000-0000-000000000002");
    private static readonly Guid IdEsp3 = new("11111111-0000-0000-0000-000000000003");

    // IDs de funciones fijas (para órdenes específicas)
    private static readonly Guid IdFun1 = new("44444444-0000-0000-0000-000000000001");
    private static readonly Guid IdFun2 = new("44444444-0000-0000-0000-000000000002");
    private static readonly Guid IdFun3 = new("44444444-0000-0000-0000-000000000003");

    // IDs de sillas fijas
    private static readonly Guid IdSillaA1_Sal1 = new("99990001-0000-0000-0000-000000000001");
    private static readonly Guid IdSillaA2_Sal1 = new("99990001-0000-0000-0000-000000000002");
    private static readonly Guid IdSillaA3_Sal1 = new("99990001-0000-0000-0000-000000000003");
    private static readonly Guid IdSillaA1_Sal2 = new("99990002-0000-0000-0000-000000000001");
    private static readonly Guid IdSillaA2_Sal2 = new("99990002-0000-0000-0000-000000000002");
    private static readonly Guid IdSillaA1_Sal3 = new("99990003-0000-0000-0000-000000000001");
    private static readonly Guid IdSillaA2_Sal3 = new("99990003-0000-0000-0000-000000000002");

    // IDs fijos de órdenes de prueba (usadas en Financiero)
    public static readonly Guid IdOrden1 = new("77777777-0000-0000-0000-000000000001");
    public static readonly Guid IdOrden2 = new("77777777-0000-0000-0000-000000000002");
    public static readonly Guid IdOrden3 = new("77777777-0000-0000-0000-000000000003");

    // Producto fijo para órdenes
    public static readonly Guid IdProd1 = new("55555555-0000-0000-0000-000000000001");
    public static readonly Guid IdProd2 = new("55555555-0000-0000-0000-000000000002");

    private static readonly (string nombre, decimal precio)[] ProductosConfiteria = [
        ("Palomitas Grandes", 12000),
        ("Palomitas Medianas", 9000),
        ("Palomitas Pequeñas", 6500),
        ("Gaseosa 500ml", 7000),
        ("Gaseosa 750ml", 9500),
        ("Agua Mineral 500ml", 3000),
        ("Jugo Natural 400ml", 5000),
        ("Perro Caliente", 9000),
        ("Nachos con Queso", 11000),
        ("Chocolate Caliente", 7500),
        ("Café Americano", 5500),
        ("Candy Bar Mix", 8000),
        ("M&M's Grande", 6000),
        ("Maltesers Pack", 5500),
        ("Gominolas Varias", 4000),
        ("Papas Fritas Grandes", 6000),
        ("Papas Fritas Pequeñas", 4000),
        ("Galletas Surtidas", 5000),
        ("Sándwich de Jamón", 10000),
        ("Sándwich de Pollo", 11000),
    ];

    public static async Task SeedAsync(VentasDbContext db)
    {
        if (await db.Productos.AnyAsync()) return;

        var ahora = DateTime.UtcNow;
        var random = new Random(42);

        // ─── Crear 20 productos de confitería ───
        var productosIds = new List<Guid>();
        var productosFixos = new Dictionary<int, Guid>
        {
            [0] = IdProd1,
            [1] = IdProd2,
        };

        foreach (var (nombre, precio) in ProductosConfiteria)
        {
            var idx = productosIds.Count;
            var id = productosFixos.TryGetValue(idx, out var fid) ? fid : Guid.NewGuid();
            productosIds.Add(id);

            var stock = random.Next(50, 300);
            var producto = ProductoConfiteria.Restore(ProductoId.Of(id), nombre, Money.Of(precio), stock);
            await db.Productos.AddAsync(producto);
        }

        await db.SaveChangesAsync();

        // ─── Crear 5 combos ───
        var combosIds = new List<Guid>();
        var combosConfig = new[]
        {
            ("Combo Clásico", new[] { 0, 3 }, 15500m), // Palomitas + Gaseosa
            ("Combo Familiar", new[] { 1, 4, 7 }, 27000m), // Palomitas med + Gaseosa 750 + Perro
            ("Combo Dulce", new[] { 12, 13, 14 }, 12000m), // Candy mix
            ("Combo VIP", new[] { 0, 4, 8, 19 }, 40000m), // Palomitas + Gaseosa grande + Nachos + Sándwich
            ("Combo Infantil", new[] { 2, 5, 14 }, 8500m), // Palomitas pequeñas + Agua + Gominolas
        };

        foreach (var (nombre, indices, precio) in combosConfig)
        {
            var id = Guid.NewGuid();
            combosIds.Add(id);

            var items = indices.Select(idx => new ComboItem(
                ProductoId.Of(productosIds[idx]),
                1,
                Money.Of(ProductosConfiteria[idx].precio))).ToList();

            var combo = DefCombo.Restore(DefComboId.Of(id), nombre, Money.Of(precio), true, items);
            await db.DefCombos.AddAsync(combo);
        }

        await db.SaveChangesAsync();

        // ─── Crear 3 órdenes de prueba (con IDs fijos para Financiero) ───
        var ordenesEspeciales = new[]
        {
            CreateOrder(IdOrden1, IdEsp1, IdFun1, [IdSillaA1_Sal1, IdSillaA2_Sal1], 0.05m, NivelOrigen.Normal,
                EstadoOrden.Confirmada, ahora.AddDays(30)),
            CreateOrder(IdOrden2, IdEsp2, IdFun2, [IdSillaA1_Sal3], 0.10m, NivelOrigen.Oro,
                EstadoOrden.Pendiente, ahora.AddMinutes(12)),
            CreateOrder(IdOrden3, IdEsp3, IdFun3, [IdSillaA1_Sal2, IdSillaA2_Sal2], 0.20m, NivelOrigen.Platino,
                EstadoOrden.Cancelada, ahora.AddDays(30)),
        };

        foreach (var orden in ordenesEspeciales)
        {
            await db.Ordenes.AddAsync(orden);
        }

        // ─── Crear ~50 órdenes adicionales aleatorias ───
        var clientes = Enumerable.Range(0, 150).Select(i => Guid.Parse($"11111111-0000-0000-0000-{i:D15}")).ToList();
        var funciones = Enumerable.Range(0, 80).Select(i => Guid.Parse($"44444444-0000-0000-0000-{i:D15}")).ToList();
        var sillas = Enumerable.Range(0, 1000).Select(i => Guid.Parse($"99990000-0000-0000-0000-{i:D15}")).ToList();

        for (int i = 0; i < 50; i++)
        {
            var clienteIdx = random.Next(clientes.Count);
            var funcionIdx = random.Next(funciones.Count);
            var cliente = clientes[clienteIdx];
            var funcion = funciones[funcionIdx];

            // Seleccionar nivel random
            var nivelRandom = random.NextDouble();
            decimal descuento;
            NivelOrigen nivel;
            if (nivelRandom < 0.60)
            {
                descuento = 0.0m;
                nivel = NivelOrigen.SinSuscripcion;
            }
            else if (nivelRandom < 0.90)
            {
                descuento = 0.05m;
                nivel = NivelOrigen.Normal;
            }
            else if (nivelRandom < 0.97)
            {
                descuento = 0.10m;
                nivel = NivelOrigen.Oro;
            }
            else
            {
                descuento = 0.20m;
                nivel = NivelOrigen.Platino;
            }

            // Generar boletas
            var numSillas = random.Next(1, 5);
            var sillaRefs = Enumerable.Range(0, numSillas)
                .Select(_ => SillaRef.Of(sillas[random.Next(sillas.Count)]))
                .ToList();

            var boletas = sillaRefs
                .Select(sr => ItemBoleta.Restore(Guid.NewGuid(),
                    FuncionRef.Of(funcion),
                    sr,
                    Money.Of(random.Next(15000, 40000)),
                    (TipoBoleta)random.Next(0, 3)))
                .ToList();

            // Generar items de confitería (50% probabilidad)
            var confiterias = new List<ItemConfiteria>();
            if (random.Next(100) < 50)
            {
                var numItems = random.Next(1, 4);
                for (int j = 0; j < numItems; j++)
                {
                    var prodIdx = random.Next(productosIds.Count);
                    var cantidad = random.Next(1, 3);
                    confiterias.Add(ItemConfiteria.Restore(Guid.NewGuid(),
                        ProductoId.Of(productosIds[prodIdx]),
                        cantidad,
                        Money.Of(ProductosConfiteria[prodIdx].precio),
                        null));
                }
            }

            var estado = random.Next(100) < 70 ? EstadoOrden.Confirmada :
                         random.Next(100) < 80 ? EstadoOrden.Pendiente :
                         random.Next(100) < 90 ? EstadoOrden.Expirada : EstadoOrden.Cancelada;

            var expiracionDays = estado == EstadoOrden.Pendiente ? random.Next(1, 15) : random.Next(20, 90);
            var subtotalBoletas = boletas.Aggregate(Money.Zero(), (acc, b) => acc.Add(b.PrecioBase));
            var subtotalConf = confiterias.Aggregate(Money.Zero(), (acc, c) => acc.Add(c.Subtotal()));
            var subtotal = subtotalBoletas.Add(subtotalConf);
            var total = subtotal.Multiply(1m - descuento);

            var orden = Orden.Restore(
                OrdenId.New(),
                EspectadorRef.Of(cliente),
                Descuento.Of(descuento, nivel),
                Expiracion.Of(ahora.AddDays(expiracionDays), ahora.AddDays(-1)),
                estado,
                total,
                boletas,
                confiterias);

            await db.Ordenes.AddAsync(orden);
        }

        // Guardar en lotes
        const int BatchSize = 10;
        var ordenes = await db.Ordenes.ToListAsync();
        if (ordenes.Count > 54) // Ya tenemos 3 + algunos más, evitar duplicados
        {
            return;
        }

        await db.SaveChangesAsync();
    }

    private static Orden CreateOrder(Guid ordenId, Guid espectadorId, Guid funcionId,
        Guid[] sillaIds, decimal descuento, NivelOrigen nivel, EstadoOrden estado, DateTime expiracion)
    {
        var boletas = sillaIds
            .Select(sr => ItemBoleta.Restore(Guid.NewGuid(),
                FuncionRef.Of(funcionId),
                SillaRef.Of(sr),
                Money.Of(25000m),
                TipoBoleta.General))
            .ToList();

        var confiterias = new List<ItemConfiteria>();
        if (estado == EstadoOrden.Confirmada)
        {
            confiterias.Add(ItemConfiteria.Restore(Guid.NewGuid(),
                ProductoId.Of(IdProd1), 1, Money.Of(12000m), null));
        }

        var subtotalBoletas = boletas.Aggregate(Money.Zero(), (acc, b) => acc.Add(b.PrecioBase));
        var subtotalConf = confiterias.Aggregate(Money.Zero(), (acc, c) => acc.Add(c.Subtotal()));
        var subtotal = subtotalBoletas.Add(subtotalConf);
        var total = subtotal.Multiply(1m - descuento);

        return Orden.Restore(
            OrdenId.Of(ordenId),
            EspectadorRef.Of(espectadorId),
            Descuento.Of(descuento, nivel),
            Expiracion.Of(expiracion, DateTime.UtcNow.AddDays(-1)),
            estado,
            total,
            boletas,
            confiterias);
    }
}
