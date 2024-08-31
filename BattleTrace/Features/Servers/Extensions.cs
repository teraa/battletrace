using System.Threading.RateLimiting;
using BattleTrace.Common;
using MediatR;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Refit;
using Teraa.Extensions.AspNetCore;
using Teraa.Extensions.Configuration;
using Index = BattleTrace.Features.Servers.Actions.Index;

namespace BattleTrace.Features.Servers;

public static class Extensions
{
    public static RouteGroupBuilder MapServers(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/servers");

        group.MapGet(
            "",
            async ([AsParameters] Index.Query query, ISender sender, CancellationToken cancellationToken)
                => await sender.Send(query, cancellationToken)
        );

        return group;
    }

    public static IServiceCollection AddServerFetcher(this IServiceCollection services)
    {
        string name = nameof(IBattlelogApi);

        services
            .AddValidatedOptions<ServerFetcherOptions>()
            .AddRefitClient<IBattlelogApi>()
            .ConfigureHttpClient(
                client =>
                {
                    client.BaseAddress = new Uri("https://battlelog.battlefield.com");
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                }
            )
            .AddKeyedHttpMessageHandler<RateLimitingHandler>(key: name)
            .AddTransientHttpErrorPolicy(
                policy => policy
                    .WaitAndRetryAsync(
                        Backoff.DecorrelatedJitterBackoffV2(
                            medianFirstRetryDelay: TimeSpan.FromSeconds(1),
                            retryCount: 3,
                            fastFirst: true
                        )
                    )
            )
            .Services
            .AddKeyedTransient<RateLimitingHandler>(
                serviceKey: name,
                (sp, _) =>
                {
                    var options = sp.GetRequiredService<IOptions<ServerFetcherOptions>>();
                    return new RateLimitingHandler(new TokenBucketRateLimiter(options.Value.RateLimiterOptions));
                }
            )
            .AddScoped<FetchServers>();

        return services;
    }
}
