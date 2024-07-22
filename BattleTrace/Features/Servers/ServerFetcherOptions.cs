using System.Threading.RateLimiting;
using BattleTrace.Common;
using FluentValidation;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Refit;
using Teraa.Extensions.AspNetCore;
using Teraa.Extensions.Configuration;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace BattleTrace.Features.Servers;

#pragma warning disable CS8618
public class ServerFetcherOptions
{
    public string Cron { get; init; } = "0 */12 * * *";
    public int Offset { get; init; } = 45;
    public int Threshold { get; init; } = 10;

    public TokenBucketRateLimiterOptions RateLimiterOptions { get; init; } = new()
    {
        ReplenishmentPeriod = TimeSpan.FromSeconds(0.5),
        TokensPerPeriod = 1,
        TokenLimit = 1,
        QueueLimit = int.MaxValue,
    };

    [UsedImplicitly]
    public class Validator : AbstractValidator<ServerFetcherOptions>
    {
        public Validator()
        {
            RuleFor(x => x.Cron).ValidCronExpression();
            RuleFor(x => x.Offset).GreaterThan(0);
            RuleFor(x => x.Threshold).GreaterThanOrEqualTo(0);
            RuleFor(x => x.RateLimiterOptions)
                .NotNull()
                .SetValidator(new TokenBucketRateLimiterOptionsValidator());
        }
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServerFetcher(this IServiceCollection services)
    {
        string name = nameof(IBattlelogApi);

        services
            .AddValidatedOptions<ServerFetcherOptions>()
            .AddRefitClient<IBattlelogApi>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("https://battlelog.battlefield.com");
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
                var options = sp.GetRequiredService<IOptions<ServerFetcherOptions>>();
                return new RateLimitingHandler(new TokenBucketRateLimiter(options.Value.RateLimiterOptions));
            })
            .AddAsyncInitializer<ServerFetcherJobInitializer>();

        return services;
    }
}
