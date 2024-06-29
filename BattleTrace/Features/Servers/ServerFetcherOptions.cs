using System.Threading.RateLimiting;
using FluentValidation;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Teraa.Extensions.AspNetCore;
using Teraa.Extensions.Configuration;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace BattleTrace.Features.Servers;

#pragma warning disable CS8618
public class ServerFetcherOptions
{
    public TimeSpan Interval { get; init; } = TimeSpan.FromHours(12);
    public TimeSpan Delay { get; init; } = TimeSpan.FromSeconds(0.5);
    public int Offset { get; init; } = 45;
    public int Threshold { get; init; } = 10;

    [UsedImplicitly]
    public class Validator : AbstractValidator<ServerFetcherOptions>
    {
        public Validator()
        {
            RuleFor(x => x.Interval).GreaterThan(TimeSpan.Zero);
            RuleFor(x => x.Delay).GreaterThan(TimeSpan.Zero);
            RuleFor(x => x.Offset).GreaterThan(0);
            RuleFor(x => x.Threshold).GreaterThanOrEqualTo(0);
        }
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServerFetcher(this IServiceCollection services)
    {
        const string key = "server";

        services
            .AddValidatedOptions<ServerFetcherOptions>()
            .AddHostedService<ServerFetcherService>()
            .AddKeyedSingleton<RateLimitingHandler>(key, (sp, _) =>
            {
                var options = sp.GetRequiredService<IOptions<ServerFetcherOptions>>();

                return new RateLimitingHandler(new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
                {
                    ReplenishmentPeriod = options.Value.Delay,
                    TokensPerPeriod = 1,
                    TokenLimit = 1,
                    QueueLimit = int.MaxValue,
                }));
            })
            .AddHttpClient<Client>(typeof(Client).FullName!)
            .AddKeyedHttpMessageHandler<RateLimitingHandler>(key);

        return services;
    }
}
