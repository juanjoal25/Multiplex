using System.Net.Http.Json;
using Multiplex.Web.Models.Dtos;

namespace Multiplex.Web.Services;

public interface IInfraestructuraClient
{
    Task<SalaDetail?> GetSalaAsync(Guid idSala, CancellationToken ct = default);
    Task<DisponibilidadResult?> ConsultarDisponibilidadAsync(Guid idSala, CancellationToken ct = default);
    Task EnviarMantenimientoAsync(Guid idSala, CancellationToken ct = default);
    Task ReactivarAsync(Guid idSala, CancellationToken ct = default);
    Task ReservarSillaAsync(Guid idSilla, ReservarSillaRequest req, CancellationToken ct = default);
    Task LiberarSillaAsync(Guid idSilla, LiberarSillaRequest req, CancellationToken ct = default);
    Task<SillaDetail?> GetSillaAsync(Guid idSilla, CancellationToken ct = default);

    // BFF-stubbed (API gap)
    IReadOnlyCollection<SalaSummary> ListarSalasStub();
}

public sealed class InfraestructuraClient(HttpClient http) : IInfraestructuraClient
{
    public async Task<SalaDetail?> GetSalaAsync(Guid idSala, CancellationToken ct = default)
    {
        var res = await http.GetAsync($"v1/infraestructura/salas/{idSala}", ct);
        if (res.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<SalaDetail>(cancellationToken: ct);
    }

    public async Task<DisponibilidadResult?> ConsultarDisponibilidadAsync(Guid idSala, CancellationToken ct = default)
    {
        var res = await http.GetAsync($"v1/infraestructura/salas/{idSala}/disponibilidad", ct);
        if (res.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<DisponibilidadResult>(cancellationToken: ct);
    }

    public async Task EnviarMantenimientoAsync(Guid idSala, CancellationToken ct = default)
    {
        var res = await http.PostAsync($"v1/infraestructura/salas/{idSala}/mantenimiento", content: null, ct);
        res.EnsureSuccessStatusCode();
    }

    public async Task ReactivarAsync(Guid idSala, CancellationToken ct = default)
    {
        var res = await http.PostAsync($"v1/infraestructura/salas/{idSala}/reactivar", content: null, ct);
        res.EnsureSuccessStatusCode();
    }

    public async Task ReservarSillaAsync(Guid idSilla, ReservarSillaRequest req, CancellationToken ct = default)
    {
        var res = await http.PutAsJsonAsync($"v1/infraestructura/sillas/{idSilla}/reservar", req, ct);
        res.EnsureSuccessStatusCode();
    }

    public async Task LiberarSillaAsync(Guid idSilla, LiberarSillaRequest req, CancellationToken ct = default)
    {
        var res = await http.PostAsJsonAsync($"v1/infraestructura/sillas/{idSilla}/liberar", req, ct);
        res.EnsureSuccessStatusCode();
    }

    public async Task<SillaDetail?> GetSillaAsync(Guid idSilla, CancellationToken ct = default)
    {
        var res = await http.GetAsync($"v1/infraestructura/sillas/{idSilla}", ct);
        if (res.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<SillaDetail>(cancellationToken: ct);
    }

    // TODO: requires GET /v1/infraestructura/salas endpoint
    public IReadOnlyCollection<SalaSummary> ListarSalasStub() => SalasStub.All;
}

internal static class SalasStub
{
    public static readonly IReadOnlyList<SalaSummary> All = new[]
    {
        new SalaSummary(Guid.Parse("22222222-2222-2222-2222-222222222201"), "Sala 1", "General", "Activa", 120),
        new SalaSummary(Guid.Parse("22222222-2222-2222-2222-222222222202"), "Sala 2 — IMAX", "Imax", "Activa", 180),
        new SalaSummary(Guid.Parse("22222222-2222-2222-2222-222222222203"), "Sala 3 — VIP", "Vip", "Activa", 48),
        new SalaSummary(Guid.Parse("22222222-2222-2222-2222-222222222204"), "Sala 4 — 4DX", "Cuatrodx", "Activa", 90),
        new SalaSummary(Guid.Parse("22222222-2222-2222-2222-222222222205"), "Sala 5", "General", "Mantenimiento", 120),
    };
}
