using Clientes.Domain.ValueObjects;
using MediatR;

namespace Clientes.Application.UseCases.RegistrarEspectador;

public sealed record RegistrarEspectadorCommand(
    string Nombre,
    string Apellido,
    string Correo,
    TipoDocumento TipoDocumento,
    string NumeroDocumento
) : IRequest<RegistrarEspectadorResult>;

public sealed record RegistrarEspectadorResult(Guid IdEspectador);
