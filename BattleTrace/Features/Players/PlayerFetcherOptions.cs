using System.Threading.RateLimiting;
using FluentValidation;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Teraa.Extensions.AspNetCore;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace BattleTrace.Features.Players;

#pragma warning disable CS8618
public class PlayerFetcherOptions
{
    public TimeSpan Interval { get; init; } = TimeSpan.FromMinutes(5);
    public TimeSpan BatchDelay { get; init; } = TimeSpan.FromSeconds(1);
    public int BatchSize { get; init; } = 30;
    public TimeSpan MaxServerAge { get; set; } = TimeSpan.FromDays(2);

    [UsedImplicitly]
    public class Validator : AbstractValidator<PlayerFetcherOptions>
    {
        public Validator()
        {
            RuleFor(x => x.Interval).GreaterThan(TimeSpan.Zero);
            RuleFor(x => x.BatchDelay).GreaterThan(TimeSpan.Zero);
            RuleFor(x => x.BatchSize).GreaterThan(0);
            RuleFor(x => x.MaxServerAge).GreaterThan(TimeSpan.Zero);
        }
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlayerFetcher(this IServiceCollection services)
    {
        const string key = "player";

        services
            .AddSingleton<PlayerFetcherService>()
            .AddHostedService<PlayerFetcherService>()
            .AddKeyedSingleton<RateLimitingHandler>(key, (sp, _) =>
            {
                var options = sp.GetRequiredService<IOptions<PlayerFetcherOptions>>();

                return new RateLimitingHandler(new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
                {
                    ReplenishmentPeriod = options.Value.BatchDelay,
                    TokensPerPeriod = options.Value.BatchSize,
                    TokenLimit = options.Value.BatchSize,
                    QueueLimit = int.MaxValue,
                }));
            })
            .AddHttpClient<Client>(typeof(Client).FullName!)
            .AddKeyedHttpMessageHandler<RateLimitingHandler>(key);

        return services;
    }
}
