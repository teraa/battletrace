using System.Threading.RateLimiting;
using BattleTrace.Common;
using FluentValidation;
using JetBrains.Annotations;

namespace BattleTrace.Features.Players;

public sealed class PlayerFetcherOptions
{
    public TimeSpan MaxServerAge { get; init; } = TimeSpan.FromDays(2);

    public TokenBucketRateLimiterOptions RateLimiterOptions { get; init; } = new()
    {
        ReplenishmentPeriod = TimeSpan.FromSeconds(1),
        TokensPerPeriod = 30,
        TokenLimit = 30,
        QueueLimit = int.MaxValue,
    };

    [UsedImplicitly]
    public sealed class Validator : AbstractValidator<PlayerFetcherOptions>
    {
        public Validator()
        {
            RuleFor(x => x.MaxServerAge).GreaterThan(TimeSpan.Zero);
            RuleFor(x => x.RateLimiterOptions)
                .NotNull()
                .SetValidator(validator: new TokenBucketRateLimiterOptionsValidator());
        }
    }
}
