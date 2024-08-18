using BattleTrace.Common;
using FluentValidation;

namespace BattleTrace.Hangfire;

public sealed class HangfireOptions
{
    public string PlayersCron { get; init; } = "*/5 * * * *";
    public string ServersCron { get; init; } = "0 */12 * * *";

    public sealed class Validator : AbstractValidator<HangfireOptions>
    {
        public Validator()
        {
            RuleFor(x => x.PlayersCron).ValidCronExpression();
            RuleFor(x => x.ServersCron).ValidCronExpression();
        }
    }
}
