using BattleTrace.Data;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Options;

namespace BattleTrace;

public static class HangfireExtensions
{
    public static IServiceCollection AddHangfire(this IServiceCollection services)
    {
        return services
            .AddHangfire(static (services, config) =>
            {
                config.UsePostgreSqlStorage(options =>
                {
                    var dbOptions = services.GetRequiredService<IOptions<DbOptions>>().Value;
                    options.UseNpgsqlConnection(dbOptions.ConnectionString);
                });
            })
            .AddHangfireServer();
    }

    public static IEndpointConventionBuilder MapHangfire(this WebApplication app)
    {
        if (app.Environment.IsEnvironment("Test"))
            return app.MapGet("/hangfire", () => "test");

        return app.MapHangfireDashboard(new DashboardOptions
        {
            Authorization = [],
            AsyncAuthorization = [],
        });
    }
}
