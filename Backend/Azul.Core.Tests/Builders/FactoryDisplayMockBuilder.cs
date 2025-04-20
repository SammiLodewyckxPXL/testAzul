using Azul.Core.Tests.Extensions;
using Azul.Core.TileFactoryAggregate.Contracts;

namespace Azul.Core.Tests.Builders;

public class FactoryDisplayMockBuilder : MockBuilder<IFactoryDisplay>
{
    public FactoryDisplayMockBuilder()
    {
        Mock.SetupGet(tc => tc.Id).Returns(Guid.NewGuid);
        Mock.SetupGet(tc => tc.Tiles).Returns(new List<TileType>());
        Mock.SetupGet(tc => tc.IsEmpty).Returns(true);
    }

    public FactoryDisplayMockBuilder WithTiles(int number = 4)
    {
        TileType[] tiles = new TileType[number];
        for (int i = 0; i < number; i++)
        {
            tiles[i] = Random.Shared.NextTileType();
        }

        Mock.SetupGet(tc => tc.Tiles).Returns(tiles.ToList());
        Mock.SetupGet(tc => tc.IsEmpty).Returns(false);
        return this;
    }

    public FactoryDisplayMockBuilder Empty()
    {
        Mock.SetupGet(tc => tc.Tiles).Returns(new List<TileType>());
        Mock.SetupGet(tc => tc.IsEmpty).Returns(true);
        return this;
    }
}