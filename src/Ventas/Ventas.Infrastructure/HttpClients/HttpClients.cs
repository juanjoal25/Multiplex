using System.Net.Http.Json;
using Ventas.Application.Abstractions;

namespace Ventas.Infrastructure.HttpClients;

public sealed class ClientesHttpClient(HttpClient http) : IClientesClient
{
    private sealed record DescuentoDto(decimal Porcentaje, string Nivel, string Estado);
    public async Task<DescuentoEspectadorInfo?> ConsultarDescuentoAsync(Guid idEspectador, CancellationToken ct = default)
    {
        try
        {
            var d = await http.GetFromJsonAsync<DescuentoDto>($"/v1/clientes/{idEspectador}/descuento", ct);
            return d is null ? null : new(d.Porcentaje, d.Nivel, d.Estado);
        }
        catch { return null; }
    }
}

public sealed class ProgramacionHttpClient(HttpClient http) : IProgramacionClient
{
    private sealed record FuncionDto(Guid Id, Guid IdSala, string TipoSala, string Formato, decimal PrecioExtraFormato, DateTime Inicio, DateTime Fin);
    public async Task<FuncionInfo?> ConsultarFuncionAsync(Guid idFuncion, CancellationToken ct = default)
    {
        try
        {
            var f = await http.GetFromJsonAsync<FuncionDto>($"/v1/programacion/funcion/{idFuncion}", ct);
            return f is null ? null : new(f.Id, f.IdSala, f.TipoSala, f.Formato, f.PrecioExtraFormato, f.Inicio, f.Fin);
        }
        catch { return null; }
    }
}

public sealed class InfraestructuraHttpClient(HttpClient http) : IInfraestructuraClient
{
    private sealed record SillaDto(Guid Id, string Tipo, string Estado);

    public async Task<SillaInfo?> ConsultarSillaAsync(Guid idSilla, CancellationToken ct = default)
    {
        try
        {
            var s = await http.GetFromJsonAsync<SillaDto>($"/v1/infraestructura/sillas/{idSilla}", ct);
            return s is null ? null : new(s.Id, s.Tipo, string.Equals(s.Estado, "Disponible", StringComparison.OrdinalIgnoreCase));
        }
        catch { return null; }
    }

    public async Task<bool> ReservarSillaAsync(Guid idSilla, Guid idFuncion, Guid idOrden, DateTime expiracion, CancellationToken ct = default)
    {
        var resp = await http.PutAsJsonAsync($"/v1/infraestructura/sillas/{idSilla}/reservar",
            new { idFuncion, idOrden, expiracion }, ct);
        return resp.IsSuccessStatusCode;
    }

    public async Task LiberarSillaAsync(Guid idSilla, string motivo, CancellationToken ct = default)
    {
        await http.PostAsJsonAsync($"/v1/infraestructura/sillas/{idSilla}/liberar", new { motivo }, ct);
    }
}

public sealed class CadenaHttpClient(HttpClient http) : ICadenaClient
{
    private sealed record ContratoDto(Guid Id, string Tercero, string Estado);
    public async Task<bool> ContratoVigenteAsync(string tercero, CancellationToken ct = default)
    {
        try
        {
            var resp = await http.GetAsync($"/v1/cadena/contratos?tercero={Uri.EscapeDataString(tercero)}", ct);
            return resp.IsSuccessStatusCode;
        }
        catch { return false; }
    }
}
