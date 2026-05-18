using MediatR;
using Ventas.Domain.Repositories;

namespace Ventas.Application.UseCases.ConsultarProductos;

public sealed record ProductoDto(Guid IdProducto, string Nombre, decimal Precio, int Stock);
public sealed record ConsultarProductosQuery : IRequest<IReadOnlyCollection<ProductoDto>>;

public sealed class ConsultarProductosHandler(IProductoRepository repo)
    : IRequestHandler<ConsultarProductosQuery, IReadOnlyCollection<ProductoDto>>
{
    public async Task<IReadOnlyCollection<ProductoDto>> Handle(ConsultarProductosQuery q, CancellationToken ct)
    {
        // Implementación de listado en infraestructura; aquí stub que requiere extensión del repo
        // Por simplicidad esta query queda como contrato a implementar concretamente en la fase 3
        await Task.CompletedTask;
        return Array.Empty<ProductoDto>();
    }
}
