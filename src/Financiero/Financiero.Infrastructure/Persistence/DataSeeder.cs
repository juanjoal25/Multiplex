using Financiero.Domain.Aggregates.TransaccionAgg;
using Financiero.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel.ValueObjects;

namespace Financiero.Infrastructure.Persistence;

public static class DataSeeder
{
    // IDs de órdenes (deben coincidir con Ventas.DataSeeder)
    private static readonly Guid IdOrden1 = new("77777777-0000-0000-0000-000000000001"); // Confirmada
    private static readonly Guid IdOrden2 = new("77777777-0000-0000-0000-000000000002"); // Pendiente
    private static readonly Guid IdOrden3 = new("77777777-0000-0000-0000-000000000003"); // Cancelada (sin transacción)

    private static readonly string[] ReferenciasBanco = [
        "VISA", "MASTERCARD", "AMEX", "DINERS", "PSE-ACH"
    ];

    private static readonly MetodoPago[] MetodosPago = [
        MetodoPago.TarjetaCredito,
        MetodoPago.TarjetaDebito,
        MetodoPago.EfectivoTaquilla,
        MetodoPago.BilleteraDigital
    ];

    public static async Task SeedAsync(FinancieroDbContext db)
    {
        if (await db.Transacciones.AnyAsync()) return;

        var ahora = DateTime.UtcNow;
        var random = new Random(42);
        var transacciones = new List<Transaccion>();

        // ─── Transacciones fijas para órdenes de prueba ───
        // Transacción 1: orden1 (Confirmada) → Aprobado
        var ordenDep1 = OrdenDepurada.Of(
            IdOrden1,
            [
                ConceptoFacturable.Of("2x Boleta Spider-Man General", Money.Of(50000m)),
                ConceptoFacturable.Of("Descuento Normal 5%", Money.Of(2500m)),
            ],
            [0.05m],
            Money.Of(47500m));

        var trans1 = Transaccion.Restore(
            TransaccionId.New(),
            ordenDep1,
            MetodoPago.TarjetaCredito,
            EstadoPago.Aprobado,
            ReferenciaExterna.Of("VISA-TXN-20260519-001"),
            RegistroContable.Of(TransaccionId.New(), Money.Of(47500m), EstadoPago.Aprobado,
                ReferenciaExterna.Of("VISA-TXN-20260519-001")),
            ahora.AddMinutes(-30),
            null);

        transacciones.Add(trans1);

        // Transacción 2: orden2 (Pendiente) → Pendiente
        var ordenDep2 = OrdenDepurada.Of(
            IdOrden2,
            [
                ConceptoFacturable.Of("1x Boleta Avatar IMAX", Money.Of(35000m)),
                ConceptoFacturable.Of("Confitería", Money.Of(19000m)),
                ConceptoFacturable.Of("Descuento Oro 10%", Money.Of(5400m)),
            ],
            [0.10m],
            Money.Of(48600m));

        var trans2 = Transaccion.Restore(
            TransaccionId.New(),
            ordenDep2,
            MetodoPago.BilleteraDigital,
            EstadoPago.Pendiente,
            null,
            null,
            ahora.AddMinutes(-2),
            null);

        transacciones.Add(trans2);

        // ─── ~50 transacciones adicionales aleatorias ───
        var ordenes = Enumerable.Range(0, 150).Select(i =>
            Guid.Parse($"77777777-0000-0000-0000-{i:D12}")).ToList();

        for (int i = 0; i < 50; i++)
        {
            var ordenIdx = random.Next(ordenes.Count);
            var ordenId = ordenes[ordenIdx];
            ordenes.RemoveAt(ordenIdx);

            // Si es una de las órdenes de prueba, usa valores conocidos
            if (ordenId == IdOrden1 || ordenId == IdOrden2 || ordenId == IdOrden3)
                continue;

            var monto = Money.Of(random.Next(20000, 100000));
            var metodo = MetodosPago[random.Next(MetodosPago.Length)];

            // 70% aprobadas, 20% pendientes, 10% rechazadas
            var estado = random.Next(100) < 70 ? EstadoPago.Aprobado :
                         random.Next(100) < 85 ? EstadoPago.Pendiente : EstadoPago.Rechazado;

            var numConceptos = random.Next(1, 4);
            var conceptos = new List<ConceptoFacturable>();
            for (int j = 0; j < numConceptos; j++)
            {
                var descripcion = j == 0 ? "Boletas de cine" :
                                  j == 1 ? "Confitería" : "Descuento aplicado";
                var valor = Money.Of(random.Next(5000, 50000));
                conceptos.Add(ConceptoFacturable.Of(descripcion, valor));
            }

            var descuentos = random.Next(100) < 50 ?
                new[] { (decimal)random.Next(0, 20) / 100m } :
                Array.Empty<decimal>();

            var ordenDepurada = OrdenDepurada.Of(ordenId, conceptos, descuentos, monto);

            var referenciaExterna = estado == EstadoPago.Aprobado ?
                ReferenciaExterna.Of($"{ReferenciasBanco[random.Next(ReferenciasBanco.Length)]}-TXN-{ahora:yyyyMMdd}-{i:D4}") :
                null;

            var registro = estado == EstadoPago.Aprobado ?
                RegistroContable.Of(TransaccionId.New(), monto, estado, referenciaExterna) :
                null;

            var transaccion = Transaccion.Restore(
                TransaccionId.New(),
                ordenDepurada,
                metodo,
                estado,
                referenciaExterna,
                registro,
                ahora.AddMinutes(-random.Next(1, 1440)), // Entre 1 minuto y 24 horas atrás
                null);

            transacciones.Add(transaccion);
        }

        // Guardar en lotes
        const int BatchSize = 10;
        for (int i = 0; i < transacciones.Count; i += BatchSize)
        {
            await db.Transacciones.AddRangeAsync(transacciones.Skip(i).Take(BatchSize));
            await db.SaveChangesAsync();
        }
    }
}
