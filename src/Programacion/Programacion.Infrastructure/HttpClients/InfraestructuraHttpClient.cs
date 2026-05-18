using System.Net.Http.Json;
using Programacion.Application.Abstractions;

namespace Programacion.Infrastructure.HttpClients;

public sealed class InfraestructuraHttpClient(HttpClient http) : IInfraestructuraClient
{
    private sealed record DisponibilidadDto(Guid IdSala, int Aforo, int Disponibles);
    private sealed record SalaDto(Guid Id, string Tipo, string Estado);

    public async Task<bool> SalaExisteYDisponibleAsync(Guid idSala, CancellationToken ct = default)
    {
        try
        {
            var s = await http.GetFromJsonAsync<SalaDto>($"/v1/infraestructura/salas/{idSala}", ct);
            return s is not null && string.Equals(s.Estado, "Disponible", StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    public async Task<string?> TipoSalaAsync(Guid idSala, CancellationToken ct = default)
    {
        try
        {
            var s = await http.GetFromJsonAsync<SalaDto>($"/v1/infraestructura/salas/{idSala}", ct);
            return s?.Tipo;
        }
        catch { return null; }
    }
}
