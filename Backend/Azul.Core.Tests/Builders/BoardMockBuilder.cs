using Azul.Core.BoardAggregate;
using Azul.Core.BoardAggregate.Contracts;
using Azul.Core.Tests.Extensions;
using Azul.Core.TileFactoryAggregate.Contracts;

namespace Azul.Core.Tests.Builders;

public class BoardMockBuilder : MockBuilder<IBoard>
{
    public BoardMockBuilder()
    {
        TileType[] allTypes = Enum.GetValues<TileType>().Where(t => t != TileType.StartingTile).ToArray();
        var wall = new TileSpot[5, 5];
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                int typeIndex = (i + j) % allTypes.Length;
                wall[i, j] = new TileSpot(allTypes[typeIndex]);
            }
        }

        var floorLine = new TileSpot[7];
        for (int i = 0; i < 7; i++)
        {
            floorLine[i] = new TileSpot();
        }

        Mock.SetupGet(b => b.PatternLines).Returns(PatternLineMockBuilder.BuildMany(5));
        Mock.SetupGet(b => b.Wall).Returns(wall);
        Mock.SetupGet(b => b.FloorLine).Returns(floorLine);
        Mock.SetupGet(b => b.HasCompletedHorizontalLine).Returns(false);
    }
}