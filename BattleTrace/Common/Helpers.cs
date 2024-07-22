using System.Text.RegularExpressions;
using Cronos;
using FluentValidation;

namespace BattleTrace.Common;

public static class Helpers
{
    private static readonly Regex s_escapeSymbols = new("([_%])", RegexOptions.Compiled);

    public static string StringToLikePattern(string input)
    {
        return s_escapeSymbols.Replace(input, @"\$1")
            .Replace('*', '%')
            .Replace('?', '_');
    }

    public static IRuleBuilderOptions<TOptions, string> ValidCronExpression<TOptions>(
        this IRuleBuilder<TOptions, string> ruleBuilder
    ) => ruleBuilder
        .NotEmpty()
        .Must(x => CronExpression.TryParse(x, out _))
        .WithMessage("Not a valid cron expression.");
}
