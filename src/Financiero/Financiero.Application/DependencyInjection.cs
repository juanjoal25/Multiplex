using Financiero.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Financiero.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddFinancieroApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<DependencyInjectionMarker>());
        services.AddSingleton<SvcProcesoPago>();
        services.AddSingleton<SvcRegistroContable>();
        return services;
    }
}

internal sealed class DependencyInjectionMarker { }
