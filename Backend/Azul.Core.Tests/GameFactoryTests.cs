using System.Drawing;
using Azul.Core.GameAggregate;
using Azul.Core.GameAggregate.Contracts;
using Azul.Core.PlayerAggregate.Contracts;
using Azul.Core.TableAggregate.Contracts;
using Azul.Core.Tests.Builders;
using Azul.Core.Tests.Extensions;
using Azul.Core.TileFactoryAggregate.Contracts;
using Azul.Core.UserAggregate;
using Guts.Client.Core;
using Moq;

namespace Azul.Core.Tests;

[ProjectComponentTestFixture("1TINProject", "Azul", "GameFactory",
    @"Azul.Core\GameAggregate\GameFactory.cs;")]
public class GameFactoryTests
{
    private GameFactory _gameFactory = null!;
    private ITable _table = null!;

    [SetUp]
    public void SetUp()
    {
        User userA = new UserBuilder().WithUserName("UserA").Build();
        User userB = new UserBuilder().WithUserName("UserB").Build();
        _table = new TableMockBuilder().WithSeatedUsers([userA, userB]).Object;
        _gameFactory = new GameFactory();
    }

    [MonitoredTest]
    public void Class_ShouldBeInternal_SoThatItCanOnlyBeUsedInTheCoreProject()
    {
        Assert.That(typeof(GameFactory).IsNotPublic, Is.True, "use 'internal class' instead of 'public class'");
    }

    [MonitoredTest]
    public void Class_ShouldImplement_IGameFactory()
    {
        Assert.That(typeof(GameFactory).IsAssignableTo(typeof(IGameFactory)), Is.True);
    }

    [MonitoredTest]
    public void IGameFactory_Interface_ShouldHaveCorrectMembers()
    {
        var type = typeof(IGameFactory);
        type.AssertInterfaceMethod(nameof(IGameFactory.CreateNewForTable), typeof(IGame), typeof(ITable));
    }

    [MonitoredTest]
    public void CreateNewForTable_ShouldInitializeBasicProperties()
    {
        // Act
        IGame? game = _gameFactory!.CreateNewForTable(_table);

        // Assert
        Assert.That(game.Id, Is.Not.EqualTo(Guid.Empty), "A non-empty Guid must be used for the id");
        foreach (IPlayer player in _table.SeatedPlayers)
        {
            IPlayer? matchingPlayer = game.Players.FirstOrDefault(p => p.Id == player.Id);
            Assert.That(matchingPlayer, Is.Not.Null, "Each player in the game should be one of the players seated at the table");
        }

        Assert.That(game.TileFactory, Is.Not.Null, "The tile factory should be initialized");

        Assert.That(game.TileFactory.Bag, Is.Not.Null, "The tile factory bag should be initialized");
        Assert.That(game.TileFactory.Bag.Tiles, Has.Count.EqualTo(80),
            "The tile factory bag should contain 80 tiles (20 tiles are distributed on the 5 factory displays");

        var tileGroups = game.TileFactory.Bag.Tiles
            .Concat(game.TileFactory.Displays.SelectMany(d => d.Tiles))
            .GroupBy(t => t).ToList();

        Assert.That(tileGroups.All(g => g.Count() == 20), Is.True,
            "The tile factory bag and displays should contain 20 tiles of each type");
        Assert.That(tileGroups.Count(g => g.Key == TileType.PlainBlue), Is.EqualTo(1),
            $"No plain blue tiles found in the bag and/or factory displays");
        Assert.That(tileGroups.Count(g => g.Key == TileType.PlainRed), Is.EqualTo(1),
            $"No plain red tiles found in the bag and/or factory displays");
        Assert.That(tileGroups.Count(g => g.Key == TileType.BlackBlue), Is.EqualTo(1),
            $"No black blue tiles found in the bag and/or factory displays");
        Assert.That(tileGroups.Count(g => g.Key == TileType.WhiteTurquoise), Is.EqualTo(1),
            $"No white turquoise tiles found in the bag and/or factory displays");
        Assert.That(tileGroups.Count(g => g.Key == TileType.YellowRed), Is.EqualTo(1),
            $"No yellow red tiles found in the bag and/or factory displays");

        Assert.That(game.TileFactory.Displays.Count, Is.EqualTo(5),
            "The number of factory displays should be 5 when there are 2 players seated at the table");
        Assert.That(game.TileFactory.Displays.Count, Is.EqualTo(_table.Preferences.NumberOfFactoryDisplays),
            "The number of factory displays should be derived from the table preferences");
    }
}