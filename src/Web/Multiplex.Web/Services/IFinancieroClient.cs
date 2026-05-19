using System.Net.Http.Json;
using Multiplex.Web.Models.Dtos;

namespace Multiplex.Web.Services;

public interface IFinancieroClient
{
    Task<Guid> RegistrarTransaccionAsync(RegistrarTransaccionRequest req, CancellationToken ct = default);
    Task<TransaccionDetail?> GetTransaccionAsync(Guid id, CancellationToken ct = default);
    Task RevertirAsync(Guid id, RevertirTransaccionRequest req, CancellationToken ct = default);
    Task<IReadOnlyCollection<RegistroHistorialDto>> ConsultarHistorialAsync(DateTime inicio, DateTime fin, CancellationToken ct = default);
}

public sealed class FinancieroClient(HttpClient http) : IFinancieroClient
{
    public async Task<Guid> RegistrarTransaccionAsync(RegistrarTransaccionRequest req, CancellationToken ct = default)
    {
        var res = await http.PostAsJsonAsync("v1/financiera/transacciones", req, ct);
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<IdEnvelope>(cancellationToken: ct);
        return body?.id ?? Guid.Empty;
    }

    public async Task<TransaccionDetail?> GetTransaccionAsync(Guid id, CancellationToken ct = default)
    {
        var res = await http.GetAsync($"v1/financiera/transacciones/{id}", ct);
        if (res.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<TransaccionDetail>(cancellationToken: ct);
    }

    public async Task RevertirAsync(Guid id, RevertirTransaccionRequest req, CancellationToken ct = default)
    {
        var res = await http.PostAsJsonAsync($"v1/financiera/transacciones/{id}/revertir", req, ct);
        res.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyCollection<RegistroHistorialDto>> ConsultarHistorialAsync(
        DateTime inicio, DateTime fin, CancellationToken ct = default)
    {
        var url = $"v1/financiera/historial?inicio={Uri.EscapeDataString(inicio.ToString("O"))}&fin={Uri.EscapeDataString(fin.ToString("O"))}";
        return (await http.GetFromJsonAsync<List<RegistroHistorialDto>>(url, ct))
            ?? new List<RegistroHistorialDto>();
    }

    private sealed record IdEnvelope(Guid id);
}
