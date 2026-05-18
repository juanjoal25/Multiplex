using Clientes.Application.Abstractions;

namespace Clientes.Infrastructure.HttpClients;

public sealed class FinancieroHttpClient(HttpClient http) : IFinancieroClient
{
    public async Task<bool> PagoFueAprobadoAsync(Guid idOrdenPago, CancellationToken ct = default)
    {
        try
        {
            var resp = await http.GetAsync($"/v1/financiera/transacciones/{idOrdenPago}", ct);
            return resp.IsSuccessStatusCode;
        }
        catch { return false; }
    }
}
