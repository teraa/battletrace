using FluentValidation;
using JetBrains.Annotations;

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
        services
            .AddSingleton<PlayerFetcherService>()
            .AddHostedService<PlayerFetcherService>()
            .AddSingleton<Client.Handler>()
            .AddHttpClient<Client>()
            .AddHttpMessageHandler<Client.Handler>();

        return services;
    }
}
