using Cadena.Domain.Repositories;
using Cadena.Domain.ValueObjects;
using MediatR;
using Shared.Kernel.Exceptions;

namespace Cadena.Application.UseCases.ConsultarConfiguracion;

public sealed record ParametroDto(string Clave, string Valor, string Tipo);
public sealed record ConsultarConfiguracionQuery(Guid IdSucursal) : IRequest<ConfiguracionResult>;
public sealed record ConfiguracionResult(Guid IdSucursal, string ZonaHoraria, string Moneda, IReadOnlyCollection<ParametroDto> Parametros);

public sealed class ConsultarConfiguracionHandler(ISucursalRepository repo)
    : IRequestHandler<ConsultarConfiguracionQuery, ConfiguracionResult>
{
    public async Task<ConfiguracionResult> Handle(ConsultarConfiguracionQuery q, CancellationToken ct)
    {
        var s = await repo.GetByIdAsync(SucursalId.Of(q.IdSucursal), ct)
            ?? throw new PreconditionFailedException("Sucursal no existe");
        var parametros = s.Configuracion.Parametros
            .Select(p => new ParametroDto(p.Clave, p.Valor, p.Tipo.ToString())).ToList();
        return new(s.Id.Value, s.Configuracion.ZonaHoraria, s.Configuracion.Moneda, parametros);
    }
}
