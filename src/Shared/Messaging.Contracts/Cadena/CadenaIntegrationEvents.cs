using Messaging.Contracts.Clientes;

namespace Messaging.Contracts.Cadena;

public sealed record ParametroModificadoDto(string Clave, string Valor, string Tipo);

public sealed record ConfiguracionActualizada(
    Guid IdSucursal,
    IReadOnlyCollection<ParametroModificadoDto> ParametrosModificados,
    DateTime Timestamp
) : IntegrationEvent;

public sealed record ContratoCorporativoRegistrado(
    Guid IdContrato,
    Guid IdSucursal,
    string Tercero,
    DateTime VigenciaInicio,
    DateTime VigenciaFin,
    DateTime Timestamp
) : IntegrationEvent;

public sealed record ContratoCorporativoVencido(
    Guid IdContrato,
    Guid IdSucursal,
    DateTime Timestamp
) : IntegrationEvent;

public sealed record ContratoCorporativoCancelado(
    Guid IdContrato,
    Guid IdSucursal,
    string Motivo,
    DateTime Timestamp
) : IntegrationEvent;
