using System.Drawing;
using Azul.Core.BoardAggregate.Contracts;
using Azul.Core.PlayerAggregate.Contracts;
using Azul.Core.TileFactoryAggregate.Contracts;
using Azul.Core.UserAggregate;
using Moq;

namespace Azul.Core.Tests.Builders;

public class PlayerMockBuilder : MockBuilder<IPlayer>
{
    private readonly BoardMockBuilder _boardMockBuilder;

    public Mock<IBoard> BoardMock => _boardMockBuilder.Mock;

    public PlayerMockBuilder()
    {
        Guid id = Guid.NewGuid();
        Mock.SetupGet(p => p.Id).Returns(id);
        Mock.SetupGet(p => p.Name).Returns("Player");
        _boardMockBuilder = new BoardMockBuilder();
        Mock.SetupGet(p => p.Board).Returns(_boardMockBuilder.Object);
        Mock.SetupProperty(p => p.HasStartingTile).SetReturnsDefault(false);
        Mock.SetupGet(p => p.LastVisitToPortugal).Returns(() => null);
        Mock.SetupGet(p => p.TilesToPlace).Returns(new List<TileType>());
    }

    public PlayerMockBuilder BasedOnUser(User user)
    {
        Mock.SetupGet(p => p.Id).Returns(user.Id);
        Mock.SetupGet(p => p.Name).Returns(user.UserName ?? "Unknown");
        Mock.SetupGet(p => p.LastVisitToPortugal).Returns(user.LastVisitToPortugal);
        return this;
    }

    public PlayerMockBuilder WithName(string name)
    {
        Mock.SetupGet(p => p.Name).Returns(name);
        return this;
    }

    public PlayerMockBuilder WithLastVisitToPortugal(DateOnly? lastVisitToPortugal)
    {
        Mock.SetupGet(p => p.LastVisitToPortugal).Returns(lastVisitToPortugal);
        return this;
    }

    public PlayerMockBuilder WithTilesToPlaceList(List<TileType> tilesToPlace)
    {
        Mock.SetupGet(p => p.TilesToPlace).Returns(tilesToPlace);
        return this;
    }
}