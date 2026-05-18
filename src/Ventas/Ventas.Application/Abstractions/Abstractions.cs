namespace Ventas.Application.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken ct = default) where TEvent : notnull;
}

public interface IUnitOfWork { Task<int> SaveChangesAsync(CancellationToken ct = default); }

public sealed record DescuentoEspectadorInfo(decimal Porcentaje, string Nivel, string Estado);
public sealed record FuncionInfo(Guid IdFuncion, Guid IdSala, string TipoSala, string Formato, decimal PrecioExtraFormato, DateTime Inicio, DateTime Fin);
public sealed record SillaInfo(Guid IdSilla, string TipoSilla, bool Disponible);

public interface IClientesClient
{
    Task<DescuentoEspectadorInfo?> ConsultarDescuentoAsync(Guid idEspectador, CancellationToken ct = default);
}

public interface IProgramacionClient
{
    Task<FuncionInfo?> ConsultarFuncionAsync(Guid idFuncion, CancellationToken ct = default);
}

public interface IInfraestructuraClient
{
    Task<SillaInfo?> ConsultarSillaAsync(Guid idSilla, CancellationToken ct = default);
    Task<bool> ReservarSillaAsync(Guid idSilla, Guid idFuncion, Guid idOrden, DateTime expiracion, CancellationToken ct = default);
    Task LiberarSillaAsync(Guid idSilla, string motivo, CancellationToken ct = default);
}

public interface ICadenaClient
{
    Task<bool> ContratoVigenteAsync(string tercero, CancellationToken ct = default);
}
