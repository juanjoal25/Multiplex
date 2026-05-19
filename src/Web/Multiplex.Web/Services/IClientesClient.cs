using System.Net.Http.Json;
using Multiplex.Web.Models.Dtos;

namespace Multiplex.Web.Services;

public interface IClientesClient
{
    Task<RegistrarEspectadorResult> RegistrarAsync(RegistroRequest req, CancellationToken ct = default);
    Task<ConsultarDescuentoResult?> ConsultarDescuentoAsync(Guid idEspectador, CancellationToken ct = default);
    Task AscenderAsync(Guid idEspectador, AscenderRequest req, CancellationToken ct = default);
    Task DescenderAsync(Guid idEspectador, CancellationToken ct = default);
}

public sealed class ClientesClient(HttpClient http) : IClientesClient
{
    public async Task<RegistrarEspectadorResult> RegistrarAsync(RegistroRequest req, CancellationToken ct = default)
    {
        var res = await http.PostAsJsonAsync("v1/clientes/registro", req, ct);
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<RegistrarEspectadorResult>(cancellationToken: ct))!;
    }

    public async Task<ConsultarDescuentoResult?> ConsultarDescuentoAsync(Guid idEspectador, CancellationToken ct = default)
    {
        var res = await http.GetAsync($"v1/clientes/{idEspectador}/descuento", ct);
        if (res.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<ConsultarDescuentoResult>(cancellationToken: ct);
    }

    public async Task AscenderAsync(Guid idEspectador, AscenderRequest req, CancellationToken ct = default)
    {
        var res = await http.PostAsJsonAsync($"v1/clientes/{idEspectador}/ascender", req, ct);
        res.EnsureSuccessStatusCode();
    }

    public async Task DescenderAsync(Guid idEspectador, CancellationToken ct = default)
    {
        var res = await http.PostAsync($"v1/clientes/{idEspectador}/descender", content: null, ct);
        res.EnsureSuccessStatusCode();
    }
}
