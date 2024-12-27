using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Teraa.Shared.Configuration;

namespace BattleTrace.Data;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDb(this IServiceCollection services)
    {
        services
            .AddAsyncInitializer<MigrationInitializer>()
            .AddValidatedOptions<DbOptions>()
            .AddDbContext<AppDbContext>(
                static (services, options) =>
                {
                    using var scope = services.CreateScope();
                    var dbOptions = scope.ServiceProvider
                        .GetRequiredService<IOptionsMonitor<DbOptions>>()
                        .CurrentValue;

                    options.UseNpgsql(
                        dbOptions.ConnectionString,
                        contextOptions =>
                        {
                            contextOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                        }
                    );

#if DEBUG
                    options.EnableSensitiveDataLogging();
#endif
                }
            );

        return services;
    }
}
