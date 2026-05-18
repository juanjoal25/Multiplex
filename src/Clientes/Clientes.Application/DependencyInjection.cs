using Microsoft.Extensions.DependencyInjection;

namespace Clientes.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddClientesApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<DependencyInjectionMarker>());
        services.AddSingleton<Domain.Services.SvcCalculoDescuento>();
        return services;
    }
}

internal sealed class DependencyInjectionMarker { }
