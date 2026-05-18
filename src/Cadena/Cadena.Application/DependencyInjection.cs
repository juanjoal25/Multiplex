using Cadena.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cadena.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCadenaApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<DependencyInjectionMarker>());
        services.AddSingleton<SvcPropagacionConfiguracion>();
        services.AddSingleton<SvcGestionContratos>();
        return services;
    }
}

internal sealed class DependencyInjectionMarker { }
