using System.Text.RegularExpressions;

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
}
