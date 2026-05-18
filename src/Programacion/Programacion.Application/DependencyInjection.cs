using Microsoft.Extensions.DependencyInjection;
using Programacion.Domain.Services;

namespace Programacion.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramacionApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<DependencyInjectionMarker>());
        services.AddSingleton<SvcValidacionHorario>();
        services.AddSingleton<SvcGestionCartelera>();
        return services;
    }
}

internal sealed class DependencyInjectionMarker { }
