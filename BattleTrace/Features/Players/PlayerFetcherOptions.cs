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

namespace BattleTrace.Features.Players;

#pragma warning disable CS8618
public class PlayerFetcherOptions
{
    public TimeSpan Interval { get; init; } = TimeSpan.FromMinutes(5);
    public TimeSpan MaxServerAge { get; init; } = TimeSpan.FromDays(2);

    public TokenBucketRateLimiterOptions RateLimiterOptions { get; init; } = new()
    {
        ReplenishmentPeriod = TimeSpan.FromSeconds(1),
        TokensPerPeriod = 30,
        TokenLimit = 30,
        QueueLimit = int.MaxValue,
    };

    [UsedImplicitly]
    public class Validator : AbstractValidator<PlayerFetcherOptions>
    {
        public Validator()
        {
            RuleFor(x => x.Interval).GreaterThan(TimeSpan.Zero);
            RuleFor(x => x.MaxServerAge).GreaterThan(TimeSpan.Zero);
            RuleFor(x => x.RateLimiterOptions)
                .NotNull()
                .SetValidator(validator: new TokenBucketRateLimiterOptionsValidator());
        }
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlayerFetcher(this IServiceCollection services)
    {
        string name = nameof(IKeeperBattlelogApi);

        services
            .AddValidatedOptions<PlayerFetcherOptions>()
            .AddHostedService<PlayerFetcherService>()
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
            });

        return services;
    }
}
