using Azul.Core.TileFactoryAggregate.Contracts;
using Moq;

namespace Azul.Core.Tests.Builders;

public class TileFactoryMockBuilder : MockBuilder<ITileFactory>
{
    private readonly TableCenterMockBuilder _tableCenterMockBuilder;
    private readonly List<FactoryDisplayMockBuilder> _factoryDisplayMockBuilders;

    public Mock<ITableCenter> TableCenterMock  => _tableCenterMockBuilder.Mock;

    public TileFactoryMockBuilder()
    {
        _tableCenterMockBuilder = new TableCenterMockBuilder();
        _factoryDisplayMockBuilders = new List<FactoryDisplayMockBuilder>();
        for (int i = 0; i < 5; i++)
        {
            _factoryDisplayMockBuilders.Add(new FactoryDisplayMockBuilder());
        }
        ITableCenter tableCenter = _tableCenterMockBuilder.Object;
        Mock.SetupGet(f => f.TableCenter).Returns(tableCenter);
        Mock.SetupGet(f => f.IsEmpty).Returns(true);
        var factoryDisplays = _factoryDisplayMockBuilders.Select(mb => mb.Object).ToList();
        Mock.SetupGet(f => f.Displays).Returns(factoryDisplays);
        Mock.SetupGet(f => f.Bag).Returns(new Mock<ITileBag>().Object);
        Mock.SetupGet(f => f.UsedTiles).Returns(new List<TileType>());
    }

    public TileFactoryMockBuilder CompletelyFilled()
    {
        _tableCenterMockBuilder.WithTiles(TileType.StartingTile);
        Mock.SetupGet(f => f.IsEmpty).Returns(false);
        foreach (FactoryDisplayMockBuilder factoryDisplayMockBuilder in _factoryDisplayMockBuilders)
        {
            factoryDisplayMockBuilder.WithTiles();
        }
        return this;
    }

    public TileFactoryMockBuilder Empty()
    {
        _tableCenterMockBuilder.Empty();
        Mock.SetupGet(f => f.IsEmpty).Returns(true);
        foreach (FactoryDisplayMockBuilder factoryDisplayMockBuilder in _factoryDisplayMockBuilders)
        {
            factoryDisplayMockBuilder.Empty();
        }
        return this;
    }
}