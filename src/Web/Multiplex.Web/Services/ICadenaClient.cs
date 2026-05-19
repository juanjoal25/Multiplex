using System.Net.Http.Json;
using Multiplex.Web.Models.Dtos;

namespace Multiplex.Web.Services;

public interface ICadenaClient
{
    Task<ConfiguracionResult?> GetConfiguracionAsync(Guid idSucursal, CancellationToken ct = default);
    Task ActualizarConfiguracionAsync(Guid idSucursal, ActualizarConfiguracionRequest req, CancellationToken ct = default);
    Task<Guid> RegistrarContratoAsync(CrearContratoRequest req, CancellationToken ct = default);
    Task CancelarContratoAsync(Guid idContrato, string motivo, CancellationToken ct = default);
    Task<bool> ContratoVigenteAsync(string tercero, CancellationToken ct = default);

    // BFF-stubbed (API gap)
    IReadOnlyCollection<ContratoSummary> ListarContratosStub();
    Guid SucursalPrincipalId();
}

public sealed class CadenaClient(HttpClient http) : ICadenaClient
{
    private static readonly Guid _sucursalPrincipal = Guid.Parse("44444444-4444-4444-4444-444444444401");
    public Guid SucursalPrincipalId() => _sucursalPrincipal;

    public async Task<ConfiguracionResult?> GetConfiguracionAsync(Guid idSucursal, CancellationToken ct = default)
    {
        var res = await http.GetAsync($"v1/cadena/configuracion/{idSucursal}", ct);
        if (res.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<ConfiguracionResult>(cancellationToken: ct);
    }

    public async Task ActualizarConfiguracionAsync(Guid idSucursal, ActualizarConfiguracionRequest req, CancellationToken ct = default)
    {
        var res = await http.PutAsJsonAsync($"v1/cadena/configuracion/{idSucursal}", req, ct);
        res.EnsureSuccessStatusCode();
    }

    public async Task<Guid> RegistrarContratoAsync(CrearContratoRequest req, CancellationToken ct = default)
    {
        var res = await http.PostAsJsonAsync("v1/cadena/contratos", req, ct);
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<IdEnvelope>(cancellationToken: ct);
        return body?.id ?? Guid.Empty;
    }

    public async Task CancelarContratoAsync(Guid idContrato, string motivo, CancellationToken ct = default)
    {
        var res = await http.DeleteAsync($"v1/cadena/contratos/{idContrato}?motivo={Uri.EscapeDataString(motivo)}", ct);
        res.EnsureSuccessStatusCode();
    }

    public async Task<bool> ContratoVigenteAsync(string tercero, CancellationToken ct = default)
    {
        var res = await http.GetAsync($"v1/cadena/contratos?tercero={Uri.EscapeDataString(tercero)}", ct);
        return res.IsSuccessStatusCode;
    }

    // TODO: requires GET /v1/cadena/contratos (no filter) endpoint in Cadena microservice
    public IReadOnlyCollection<ContratoSummary> ListarContratosStub() => ContratosStub.All;

    private sealed record IdEnvelope(Guid id);
}

internal static class ContratosStub
{
    public static readonly IReadOnlyList<ContratoSummary> All = new[]
    {
        new ContratoSummary(Guid.Parse("55555555-5555-5555-5555-555555555501"),
            Guid.Parse("44444444-4444-4444-4444-444444444401"),
            "Coca-Cola FEMSA", new DateTime(2026, 1, 1), new DateTime(2026, 12, 31),
            "Sponsor exclusivo de bebidas en lobby y salas.", "Vigente"),
        new ContratoSummary(Guid.Parse("55555555-5555-5555-5555-555555555502"),
            Guid.Parse("44444444-4444-4444-4444-444444444401"),
            "Cine Colombia (intercambio)", new DateTime(2025, 6, 1), new DateTime(2026, 5, 31),
            "Intercambio de catálogo independiente.", "Vigente"),
        new ContratoSummary(Guid.Parse("55555555-5555-5555-5555-555555555503"),
            Guid.Parse("44444444-4444-4444-4444-444444444401"),
            "Universidad EAFIT", new DateTime(2026, 3, 15), new DateTime(2026, 3, 15),
            "Alquiler sala 3 para premier estudiantil.", "Cumplido"),
    };
}
