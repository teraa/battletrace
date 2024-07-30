using Microsoft.Extensions.DependencyInjection;

namespace BattleTrace.Tests;

public static class Extensions
{
    public static IServiceCollection RemoveAll(
        this IServiceCollection services,
        Func<ServiceDescriptor, bool> predicate)
    {
        var descriptors = services.Where(predicate).ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }

        return services;
    }

    public static IServiceCollection RemoveService<TService>(this IServiceCollection services)
    {
        services.RemoveAll(x =>
            x.ServiceType == typeof(TService) ||
            !x.IsKeyedService &&
            (x.ImplementationType == typeof(TService) ||
             (x.ImplementationFactory is not null &&
              x.ImplementationFactory.Method.ReturnType == typeof(TService))));

        return services;
    }

    public static T GetRequiredService<T>(this IServiceScope scope)
        where T : notnull
        => scope.ServiceProvider.GetRequiredService<T>();
}
