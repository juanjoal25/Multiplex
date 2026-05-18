using Clientes.Domain.Aggregates.EspectadorAgg;
using Clientes.Domain.Repositories;
using Clientes.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Clientes.Infrastructure.Persistence;

public sealed class EspectadorRepository(ClientesDbContext db) : IEspectadorRepository
{
    public Task<Espectador?> GetByIdAsync(EspectadorId id, CancellationToken ct = default)
        => db.Espectadores.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task AddAsync(Espectador aggregate, CancellationToken ct = default)
        => await db.Espectadores.AddAsync(aggregate, ct);

    public Task UpdateAsync(Espectador aggregate, CancellationToken ct = default)
    { db.Espectadores.Update(aggregate); return Task.CompletedTask; }

    public Task DeleteAsync(Espectador aggregate, CancellationToken ct = default)
    { db.Espectadores.Remove(aggregate); return Task.CompletedTask; }

    public Task<Espectador?> GetByDocumentoAsync(Documento documento, CancellationToken ct = default)
        => db.Espectadores.FirstOrDefaultAsync(e =>
            e.Documento.Tipo == documento.Tipo && e.Documento.Numero == documento.Numero, ct);

    public Task<bool> ExistsByDocumentoAsync(Documento documento, CancellationToken ct = default)
        => db.Espectadores.AnyAsync(e =>
            e.Documento.Tipo == documento.Tipo && e.Documento.Numero == documento.Numero, ct);

    public Task<bool> ExistsByEmailAsync(Email email, CancellationToken ct = default)
        => db.Espectadores.AnyAsync(e => e.Correo.Value == email.Value, ct);
}
