using System.Threading.RateLimiting;
using BattleTrace.Common;
using FluentValidation;
using JetBrains.Annotations;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace BattleTrace.Features.Servers;

public class ServerFetcherOptions
{
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
            RuleFor(x => x.Offset).GreaterThan(0);
            RuleFor(x => x.Threshold).GreaterThanOrEqualTo(0);
            RuleFor(x => x.RateLimiterOptions)
                .NotNull()
                .SetValidator(new TokenBucketRateLimiterOptionsValidator());
        }
    }
}
