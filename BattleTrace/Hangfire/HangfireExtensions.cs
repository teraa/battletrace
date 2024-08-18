using BattleTrace.Data;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Options;
using Teraa.Extensions.Configuration;

namespace BattleTrace.Hangfire;

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
            .AddHangfireServer()
            .AddValidatedOptions<HangfireOptions>()
            .AddAsyncInitializer<HangfireJobInitializer>();
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
