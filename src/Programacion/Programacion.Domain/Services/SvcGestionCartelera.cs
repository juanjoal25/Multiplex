using Programacion.Domain.Aggregates.CarteleraAgg;
using Programacion.Domain.Aggregates.FuncionAgg;
using Programacion.Domain.States;
using Shared.Kernel.Exceptions;

namespace Programacion.Domain.Services;

public sealed class SvcGestionCartelera
{
    public void PublicarFuncion(Cartelera cartelera, Funcion funcion)
    {
        if (funcion.Estado.Tipo is EstadoFuncionTipo.Finalizada or EstadoFuncionTipo.Cancelada)
            throw new PreconditionFailedException("No se publica función finalizada/cancelada");
        cartelera.Agregar(funcion.Id);
    }

    public void RetirarFuncion(Cartelera cartelera, Funcion funcion, string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new PreconditionFailedException("Motivo requerido");
        cartelera.Retirar(funcion.Id);
    }
}
