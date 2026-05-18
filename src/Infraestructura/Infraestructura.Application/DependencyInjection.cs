using Infraestructura.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infraestructura.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddInfraestructuraApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<DependencyInjectionMarker>());
        services.AddSingleton<SvcGestionAforo>();
        services.AddSingleton<SvcLiberacionReservas>();
        services.AddSingleton<SvcCambioEstadoSala>();
        return services;
    }
}

internal sealed class DependencyInjectionMarker { }
