using Azul.Core.TileFactoryAggregate.Contracts;

namespace Azul.Core.Tests.Builders;

public class TableCenterMockBuilder : MockBuilder<ITableCenter>
{
    public TableCenterMockBuilder()
    {
        Mock.SetupGet(tc => tc.Id).Returns(Guid.NewGuid);
        Mock.SetupGet(tc => tc.Tiles).Returns(new List<TileType>());
        Mock.SetupGet(tc => tc.IsEmpty).Returns(true);
    }

    public TableCenterMockBuilder WithTiles(params TileType[] tiles)
    {
        Mock.SetupGet(tc => tc.Tiles).Returns(tiles.ToList());
        Mock.SetupGet(tc => tc.IsEmpty).Returns(false);
        return this;
    }

    public TableCenterMockBuilder Empty()
    {
        Mock.SetupGet(tc => tc.Tiles).Returns(new List<TileType>());
        Mock.SetupGet(tc => tc.IsEmpty).Returns(true);
        return this;
    }
}