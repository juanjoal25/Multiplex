using System.Net.Http.Json;
using Multiplex.Web.Models.Dtos;

namespace Multiplex.Web.Services;

public interface IProgramacionClient
{
    Task<ConsultarCarteleraResult> ConsultarCarteleraAsync(CancellationToken ct = default);
    Task<FuncionDetail?> GetFuncionAsync(Guid idFuncion, CancellationToken ct = default);
    Task<Guid> RegistrarPeliculaAsync(CrearPeliculaRequest req, CancellationToken ct = default);
    Task<Guid> ProgramarFuncionAsync(CrearFuncionRequest req, CancellationToken ct = default);
    Task CancelarFuncionAsync(Guid idFuncion, string motivo, CancellationToken ct = default);

    // BFF-stubbed (API gap)
    IReadOnlyCollection<PeliculaSummary> ListarPeliculasStub();
    PeliculaSummary? GetPeliculaStub(Guid id);
}

public sealed class ProgramacionClient(HttpClient http) : IProgramacionClient
{
    public async Task<ConsultarCarteleraResult> ConsultarCarteleraAsync(CancellationToken ct = default)
        => (await http.GetFromJsonAsync<ConsultarCarteleraResult>("v1/programacion/cartelera", ct))
            ?? new ConsultarCarteleraResult(null, Array.Empty<Guid>());

    public async Task<FuncionDetail?> GetFuncionAsync(Guid idFuncion, CancellationToken ct = default)
    {
        var res = await http.GetAsync($"v1/programacion/funcion/{idFuncion}", ct);
        if (res.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<FuncionDetail>(cancellationToken: ct);
    }

    public async Task<Guid> RegistrarPeliculaAsync(CrearPeliculaRequest req, CancellationToken ct = default)
    {
        var res = await http.PostAsJsonAsync("v1/programacion/peliculas", req, ct);
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<IdEnvelope>(cancellationToken: ct);
        return body?.id ?? Guid.Empty;
    }

    public async Task<Guid> ProgramarFuncionAsync(CrearFuncionRequest req, CancellationToken ct = default)
    {
        var res = await http.PostAsJsonAsync("v1/programacion/funciones", req, ct);
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<IdEnvelope>(cancellationToken: ct);
        return body?.id ?? Guid.Empty;
    }

    public async Task CancelarFuncionAsync(Guid idFuncion, string motivo, CancellationToken ct = default)
    {
        var res = await http.DeleteAsync($"v1/programacion/funciones/{idFuncion}?motivo={Uri.EscapeDataString(motivo)}", ct);
        res.EnsureSuccessStatusCode();
    }

    // TODO: requires GET /v1/programacion/peliculas endpoint in Programacion microservice
    public IReadOnlyCollection<PeliculaSummary> ListarPeliculasStub() => PeliculasStub.All;
    public PeliculaSummary? GetPeliculaStub(Guid id) => PeliculasStub.All.FirstOrDefault(p => p.Id == id);

    private sealed record IdEnvelope(Guid id);
}

internal static class PeliculasStub
{
    public static readonly IReadOnlyList<PeliculaSummary> All = new[]
    {
        new PeliculaSummary(Guid.Parse("11111111-1111-1111-1111-111111111101"), "La Última Función", "Drama", 125, "+15", "IMAX", 8.4m,
            "Un director enfrenta la obsesión en el límite de su arte.", "Cartelera"),
        new PeliculaSummary(Guid.Parse("11111111-1111-1111-1111-111111111102"), "Fractura", "Thriller", 108, "+15", "2D", 7.9m,
            "Una abogada investiga la verdad detrás de un caso archivado.", "Cartelera"),
        new PeliculaSummary(Guid.Parse("11111111-1111-1111-1111-111111111103"), "Horizonte Rojo", "Acción", 134, "+13", "4DX", 8.1m,
            "Una misión imposible cruza el desierto en busca del último testigo.", "Cartelera"),
        new PeliculaSummary(Guid.Parse("11111111-1111-1111-1111-111111111104"), "Los Que No Vuelven", "Terror", 98, "+18", "2D", 7.2m,
            "Una expedición de archivistas despierta lo que estaba enterrado.", "Cartelera"),
        new PeliculaSummary(Guid.Parse("11111111-1111-1111-1111-111111111105"), "Marea Alta", "Suspenso", 112, "+13", "3D", 7.5m,
            "Tres vidas convergen en una isla justo antes de la tormenta.", "Cartelera"),
        new PeliculaSummary(Guid.Parse("11111111-1111-1111-1111-111111111106"), "Código Azul", "Sci-Fi", 142, "+13", "IMAX", 0m,
            "El último piloto humano contra una IA que aprende a soñar.", "Proximamente"),
    };
}
