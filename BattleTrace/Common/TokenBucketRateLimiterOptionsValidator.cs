using System.Threading.RateLimiting;
using FluentValidation;

namespace BattleTrace.Common;

public sealed class TokenBucketRateLimiterOptionsValidator : AbstractValidator<TokenBucketRateLimiterOptions>
{
    public TokenBucketRateLimiterOptionsValidator()
    {
        RuleFor(x => x.ReplenishmentPeriod).GreaterThan(TimeSpan.Zero);
        RuleFor(x => x.TokensPerPeriod).GreaterThan(0);
        RuleFor(x => x.TokenLimit).GreaterThan(0);
        RuleFor(x => x.QueueLimit).GreaterThan(0);
    }
}
