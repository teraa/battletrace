using FluentValidation;
using JetBrains.Annotations;
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
        services
            .AddValidatedOptions<ServerFetcherOptions>()
            .AddHostedService<ServerFetcherService>()
            .AddSingleton<Client.Handler>()
            .AddHttpClient<Client>(typeof(Client).FullName!)
            .AddHttpMessageHandler<Client.Handler>();

        return services;
    }
}
