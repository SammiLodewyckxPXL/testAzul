using Azul.Core.BoardAggregate.Contracts;

namespace Azul.Core.Tests.Builders;

public class PatternLineMockBuilder : MockBuilder<IPatternLine>
{
    public PatternLineMockBuilder(int length)
    {
        Mock.SetupGet(p => p.IsComplete).Returns(false);
        Mock.SetupGet(p => p.Length).Returns(length);
        Mock.SetupGet(p => p.TileType).Returns(() => null);
        Mock.SetupGet(p => p.NumberOfTiles).Returns(0);
    }

    public static IPatternLine[] BuildMany(int number)
    {
        var patternLines = new IPatternLine[number];
        for (int i = 1; i <= number; i++)
        {
            var patternLine = new PatternLineMockBuilder(i).Object;
            patternLines[i - 1] = patternLine;
        }
        return patternLines;
    }
}