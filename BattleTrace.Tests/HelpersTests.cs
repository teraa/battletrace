using BattleTrace.Common;

namespace BattleTrace.Tests;

public class HelpersTests
{
    [Theory]
    // No change
    [InlineData("", "")]
    [InlineData("foo", "foo")]
    // Escape only _ and %
    [InlineData("_", @"\_")]
    [InlineData("%", @"\%")]
    [InlineData("*", "%")]
    [InlineData("?", "_")]
    // Escape at different position
    [InlineData("a_a", @"a\_a")]
    [InlineData("a_", @"a\_")]
    [InlineData("_a", @"\_a")]
    public void StringToLikePatternTest(string input, string expectedResult)
    {
        var result = Helpers.StringToLikePattern(input);

        result.Should().Be(expectedResult);
    }
}
