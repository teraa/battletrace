using BattleTrace.Common;
using FluentAssertions;

namespace BattleTrace.Tests;

public class HelpersTests
{
    [Theory]

    [InlineData("", "")]
    [InlineData("foo", "foo")]

    [InlineData("_", @"\_")]
    [InlineData("%", @"\%")]
    [InlineData("*", "%")]
    [InlineData("?", "_")]

    [InlineData("a_a", @"a\_a")]
    [InlineData("a_", @"a\_")]
    [InlineData("_a", @"\_a")]

    public void StringToLikePatternTest(string input, string expectedResult)
    {
        var result = Helpers.StringToLikePattern(input);

        result.Should().Be(expectedResult);
    }
}
