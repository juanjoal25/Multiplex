using System.Net.Http.Json;
using Multiplex.Web.Models.Dtos;

namespace Multiplex.Web.Services;

public interface IVentasClient
{
    Task<CrearOrdenResult> CrearOrdenAsync(CrearOrdenRequest req, CancellationToken ct = default);
    Task ConfirmarOrdenAsync(Guid idOrden, CancellationToken ct = default);
    Task CancelarOrdenAsync(Guid idOrden, string motivo, CancellationToken ct = default);

    // BFF-stubbed (API gap)
    IReadOnlyCollection<ProductoConfiteria> ListarConfiteriaStub();
    ProductoConfiteria? GetProductoStub(Guid id);
}

public sealed class VentasClient(HttpClient http) : IVentasClient
{
    public async Task<CrearOrdenResult> CrearOrdenAsync(CrearOrdenRequest req, CancellationToken ct = default)
    {
        var res = await http.PostAsJsonAsync("v1/ventas/orden", req, ct);
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<CrearOrdenResult>(cancellationToken: ct))!;
    }

    public async Task ConfirmarOrdenAsync(Guid idOrden, CancellationToken ct = default)
    {
        var res = await http.PostAsync($"v1/ventas/orden/{idOrden}/confirmar", content: null, ct);
        res.EnsureSuccessStatusCode();
    }

    public async Task CancelarOrdenAsync(Guid idOrden, string motivo, CancellationToken ct = default)
    {
        var res = await http.DeleteAsync($"v1/ventas/orden/{idOrden}?motivo={Uri.EscapeDataString(motivo)}", ct);
        res.EnsureSuccessStatusCode();
    }

    // TODO: requires GET /v1/ventas/confiteria/productos endpoint
    public IReadOnlyCollection<ProductoConfiteria> ListarConfiteriaStub() => ConfiteriaStub.All;
    public ProductoConfiteria? GetProductoStub(Guid id) => ConfiteriaStub.All.FirstOrDefault(p => p.Id == id);
}

internal static class ConfiteriaStub
{
    public static readonly IReadOnlyList<ProductoConfiteria> All = new[]
    {
        new ProductoConfiteria(Guid.Parse("33333333-3333-3333-3333-333333333301"), "Crispetas Grandes", "Snacks", "Maíz fresco, mantequilla derretida.", 18000m),
        new ProductoConfiteria(Guid.Parse("33333333-3333-3333-3333-333333333302"), "Crispetas Medianas", "Snacks", "Tamaño ideal para una persona.", 14000m),
        new ProductoConfiteria(Guid.Parse("33333333-3333-3333-3333-333333333303"), "Gaseosa 32oz", "Bebidas", "Refresco frío sin azúcar opcional.", 12000m),
        new ProductoConfiteria(Guid.Parse("33333333-3333-3333-3333-333333333304"), "Agua 500ml", "Bebidas", "Botella sellada.", 6000m),
        new ProductoConfiteria(Guid.Parse("33333333-3333-3333-3333-333333333305"), "Nachos con Queso", "Snacks", "Chips de maíz tostado, queso amarillo.", 20000m),
        new ProductoConfiteria(Guid.Parse("33333333-3333-3333-3333-333333333306"), "Combo Pareja", "Combos", "2 crispetas medianas + 2 gaseosas.", 42000m),
        new ProductoConfiteria(Guid.Parse("33333333-3333-3333-3333-333333333307"), "Combo Sala", "Combos", "4 crispetas medianas + 4 gaseosas.", 78000m),
        new ProductoConfiteria(Guid.Parse("33333333-3333-3333-3333-333333333308"), "Chocolatina", "Snacks", "Chocolate con leche, 50g.", 5000m),
    };
}
