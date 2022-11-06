using FluentValidation;
using JetBrains.Annotations;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace BattleTrace.Api.Options;

#pragma warning disable CS8618
public class FetcherOptions
{
    public TimeSpan Interval { get; init; } = TimeSpan.FromHours(12);
    public TimeSpan Delay { get; init; } = TimeSpan.FromSeconds(0.5);
    public int Offset { get; init; } = 45;
    public int Threshold { get; init; } = 10;

    [UsedImplicitly]
    public class Validator : AbstractValidator<FetcherOptions>
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
