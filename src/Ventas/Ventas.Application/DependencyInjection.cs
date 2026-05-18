using Microsoft.Extensions.DependencyInjection;
using Ventas.Domain.Services;

namespace Ventas.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddVentasApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<DependencyInjectionMarker>());
        services.AddSingleton<SvcCalculoPrecio>();
        services.AddSingleton<SvcValidacionEventoCorporativo>();
        services.AddSingleton<ParametrosPrecio>();
        return services;
    }
}

internal sealed class DependencyInjectionMarker { }
