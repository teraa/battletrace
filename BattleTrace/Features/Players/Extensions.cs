using System.Threading.RateLimiting;
using BattleTrace.Common;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Refit;
using Teraa.Extensions.AspNetCore;
using Teraa.Extensions.Configuration;

namespace BattleTrace.Features.Players;

public static class Extensions
{
    public static IServiceCollection AddPlayerFetcher(this IServiceCollection services)
    {
        string name = nameof(IKeeperBattlelogApi);

        services
            .AddValidatedOptions<PlayerFetcherOptions>()
            .AddRefitClient<IKeeperBattlelogApi>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("https://keeper.battlelog.com");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            })
            .AddKeyedHttpMessageHandler<RateLimitingHandler>(key: name)
            .AddTransientHttpErrorPolicy(policy => policy
                .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(
                    medianFirstRetryDelay: TimeSpan.FromSeconds(1),
                    retryCount: 3,
                    fastFirst: true)
                )
            )
            .Services
            .AddKeyedTransient<RateLimitingHandler>(serviceKey: name, (sp, _) =>
            {
                var options = sp.GetRequiredService<IOptions<PlayerFetcherOptions>>();
                return new RateLimitingHandler(new TokenBucketRateLimiter(options.Value.RateLimiterOptions));
            })
            .AddAsyncInitializer<PlayerFetcherJobInitializer>()
            .AddScoped<FetchPlayers>();

        return services;
    }
}
