using Azul.Core.Tests.Builders;
using Azul.Core.Tests.Extensions;
using Azul.Core.TileFactoryAggregate;
using Azul.Core.TileFactoryAggregate.Contracts;
using Guts.Client.Core;
using Moq;

namespace Azul.Core.Tests;

[ProjectComponentTestFixture("1TINProject", "Azul", "TileFactory",
    @"Azul.Core\TileFactoryAggregate\TileFactory.cs;")]
public class TileFactoryTests
{
    private ITileFactory? _tileFactory;
    private Mock<ITileBag> _bagMock = null!;
    private TileBagMockBuilder _tileBagMockBuilder = null!;

    [SetUp]
    public void SetUp()
    {
        _tileBagMockBuilder = new TileBagMockBuilder();
        _bagMock = _tileBagMockBuilder.Mock;
        _tileFactory = new TileFactory(5, _bagMock.Object) as ITileFactory;
    }

    [MonitoredTest]
    public void Class_ShouldBeInternal_SoThatItCanOnlyBeUsedInTheCoreProject()
    {
        Assert.That(typeof(TileFactory).IsNotPublic, Is.True, "use 'internal class' instead of 'public class'");
    }

    [MonitoredTest]
    public void Class_ShouldImplement_ITileFactory()
    {
        Assert.That(typeof(TileFactory).IsAssignableTo(typeof(ITileFactory)), Is.True, "TileFactory should implement ITileFactory");
    }

    [MonitoredTest]
    public void ITileFactory_Interface_ShouldHaveCorrectMembers()
    {
        var type = typeof(ITileFactory);
        type.AssertInterfaceProperty(nameof(ITileFactory.Bag), shouldHaveGetter: true, shouldHaveSetter: false);
        type.AssertInterfaceProperty(nameof(ITileFactory.Displays), shouldHaveGetter: true, shouldHaveSetter: false);
        type.AssertInterfaceProperty(nameof(ITileFactory.TableCenter), shouldHaveGetter: true, shouldHaveSetter: false);
        type.AssertInterfaceProperty(nameof(ITileFactory.UsedTiles), shouldHaveGetter: true, shouldHaveSetter: false);
        type.AssertInterfaceProperty(nameof(ITileFactory.IsEmpty), shouldHaveGetter: true, shouldHaveSetter: false);

        type.AssertInterfaceMethod(nameof(ITileFactory.FillDisplays), returnType: typeof(void));
        type.AssertInterfaceMethod(nameof(ITileFactory.TakeTiles), returnType: typeof(IReadOnlyList<TileType>), typeof(Guid), typeof(TileType));
        type.AssertInterfaceMethod(nameof(ITileFactory.AddToUsedTiles), returnType: typeof(void), typeof(TileType));
    }

    [MonitoredTest]
    public void Constructor_ShouldInitializeProperties()
    {
        Class_ShouldImplement_ITileFactory();
        Assert.That(_tileFactory!.Bag, Is.SameAs(_bagMock.Object), "Bag should be initialized");
        Assert.That(_tileFactory.Displays, Is.Not.Null, "Displays should be initialized");
        Assert.That(_tileFactory.Displays.Count, Is.EqualTo(5), "There should be 5 displays");
        Assert.That(_tileFactory.Displays.All(d => d.IsEmpty), Is.True, "All displays should be empty");
        Assert.That(_tileFactory.TableCenter, Is.Not.Null, "TableCenter should be initialized");
        Assert.That(_tileFactory.TableCenter.IsEmpty, Is.True, "TableCenter should be empty");
        Assert.That(_tileFactory.UsedTiles, Is.Not.Null, "UsedTiles should be initialized");
        Assert.That(_tileFactory.UsedTiles.Count, Is.Zero, "UsedTiles should be empty");
    }


    [MonitoredTest]
    public void FillDisplays_ShouldFillEachDisplayWithFourTiles()
    {
        Class_ShouldImplement_ITileFactory();

        // Act
        _tileFactory!.FillDisplays();

        // Assert
        _bagMock.Verify(b => b.TryTakeTiles(4, out It.Ref<IReadOnlyList<TileType>>.IsAny), Times.Exactly(5),
            "The 'TryTakeTiles' method of the bag should be called correctly 5 times (once for each display)");
        foreach (var display in _tileFactory.Displays)
        {
            Assert.That(display.Tiles.Count, Is.EqualTo(4), "Each display should have 4 tiles");
        }
        Assert.That(_tileFactory.Bag.Tiles, Has.Count.EqualTo(80), "Bag should contain 80 tiles after filling displays");
        Assert.That(_tileFactory.TableCenter.IsEmpty, Is.True, "TableCenter should be empty");
    }

    [MonitoredTest]
    public void FillDisplays_NotEnoughTilesInBag_ShouldAddUsedTilesInBagAndTryToFillRemainingDisplaySpots()
    {
        Class_ShouldImplement_ITileFactory();

        _tileBagMockBuilder.WithTiles(TileType.PlainBlue, TileType.WhiteTurquoise);

        //Add some random tiles to the used tiles
        for (int i = 0; i < 25; i++)
        {
            TileType type = Random.Shared.NextTileType();
            _tileFactory!.AddToUsedTiles(type);
        }
        
        // Act
        _tileFactory!.FillDisplays();

        // Assert
        _bagMock.Verify(b => b.TryTakeTiles(4, out It.Ref<IReadOnlyList<TileType>>.IsAny), Times.Exactly(5),
            "The 'TryTakeTiles' method of the bag should be called correctly 5 times (once for each display)");
        _bagMock.Verify(b => b.TryTakeTiles(2, out It.Ref<IReadOnlyList<TileType>>.IsAny), Times.Exactly(1),
            "The 'TryTakeTiles' method of the bag should be called once for amount = 2 " +
            "because the first time only 2 tiles are taken and another 2 have to be taken after filling the bag with used tiles");
        foreach (var display in _tileFactory.Displays)
        {
            Assert.That(display.Tiles.Count, Is.EqualTo(4), "Each display should have 4 tiles");
        }
    }

    [MonitoredTest]
    public void TakeTiles_FromOneOfTheDisplays_ShouldTakeAllTilesOfType_ShouldMoveOtherTilesToTableCenter()
    {
        Class_ShouldImplement_ITileFactory();

        // Arrange
        _tileFactory!.FillDisplays();
        IFactoryDisplay display = Random.Shared.NextItem(_tileFactory!.Displays);
        
        Assert.That(display.Tiles.Count, Is.EqualTo(4), "There should be 4 tiles on each display (after calling 'FillDisplays')");

        TileType tileType = Random.Shared.NextItem(display.Tiles);
        List<TileType> expectedTakenTiles = display.Tiles.Where(t => t == tileType).ToList();
        List<TileType> expectedMovedTiles = display.Tiles.Where(t => t != tileType).ToList();

        // Act
        IReadOnlyList<TileType> takenTiles = _tileFactory.TakeTiles(display.Id, tileType);

        // Assert
        Assert.That(takenTiles, Is.EquivalentTo(expectedTakenTiles),
            $"Tried to take tiles of type {tileType} from display {display.Id}. Not all tiles of that type were taken");
        Assert.That(expectedMovedTiles.All(t => _tileFactory.TableCenter.Tiles.Contains(t)), Is.True,
            "All tiles that were not taken should be moved to the table center");
    }

    [MonitoredTest]
    public void TakeTiles_FromTableCenter_ShouldTakeAllTilesOfTypeAndStartingTile()
    {
        Class_ShouldImplement_ITileFactory();

        // Arrange
        _tileFactory!.FillDisplays();
        _tileFactory.TableCenter.AddStartingTile();
        Assert.That(_tileFactory.TableCenter.Tiles.Count, Is.EqualTo(1),
            "There should be 1 tile in the table center (after calling 'AddStartingTile' on table center)");
        Assert.That(_tileFactory.TableCenter.Tiles[0], Is.EqualTo(TileType.StartingTile),
            "The tile in the table center should be the starting tile (after calling 'AddStartingTile' on table center)");

        foreach (IFactoryDisplay display in _tileFactory.Displays)
        {
            TileType displayTileType = Random.Shared.NextItem(display.Tiles);
            _tileFactory.TakeTiles(display.Id, displayTileType);
        }
        Assert.That(_tileFactory.TableCenter.Tiles.Count > 1, Is.True,
            "There should be more than 1 tile in the table center (after taking tiles from all displays)");
        TileType tileType = Random.Shared.NextItem(_tileFactory.TableCenter.Tiles.Where(t => t!= TileType.StartingTile));

        List<TileType> expectedTakenTiles = _tileFactory.TableCenter.Tiles.Where(t => t == tileType).ToList();
        List<TileType> expectedRemainingTiles = _tileFactory.TableCenter.Tiles.Where(t => t != tileType && t != TileType.StartingTile).ToList();

        // Act
        IReadOnlyList<TileType> takenTiles = _tileFactory.TakeTiles(_tileFactory.TableCenter.Id, tileType);

        // Assert
        Assert.That(takenTiles.Contains(TileType.StartingTile), Is.True, "Starting tile should be taken from table center");
        List<TileType> takenTilesWithoutStartingTile = takenTiles.Where(t => t != TileType.StartingTile).ToList();
        Assert.That(takenTilesWithoutStartingTile, Is.EquivalentTo(expectedTakenTiles),
            $"Tried to take tiles of type {tileType} from table center. Not all tiles of that type were taken");
        Assert.That(expectedRemainingTiles.All(t => _tileFactory.TableCenter.Tiles.Contains(t)), Is.True,
            "All tiles that were not taken should remain in the table center");
    }

    [MonitoredTest]
    public void TakeTiles_NonExistingDisplayId_ShouldThrowInvalidOperationException()
    {
        Class_ShouldImplement_ITileFactory();

        // Arrange
        _tileFactory!.FillDisplays();
        Guid nonExistingDisplayId = Guid.NewGuid();

        Assert.That(() => _tileFactory.TakeTiles(nonExistingDisplayId, TileType.PlainBlue),
            Throws.InvalidOperationException.With.Message.ContainsOne("exist", "bestaat"));
    }

    [MonitoredTest]
    public void TakeTiles_TileTypeIsNotInDisplay_ShouldThrowInvalidOperationException()
    {
        Class_ShouldImplement_ITileFactory();

        // Arrange
        _tileFactory!.FillDisplays();
        IFactoryDisplay display = Random.Shared.NextItem(_tileFactory!.Displays);
        IEnumerable<TileType> tileTypesNotInDisplay = Enum.GetValues<TileType>().Where(t => display.Tiles.All(dt => dt != t));
        TileType tileTypeNotInDisplay = Random.Shared.NextItem(tileTypesNotInDisplay);

        Assert.That(() => _tileFactory.TakeTiles(display.Id, tileTypeNotInDisplay),
            Throws.InvalidOperationException.With.Message.ContainsOne("tile", "tegel"));
    }

    [MonitoredTest]
    public void TakeTiles_TileTypeIsNotInTableCenter_ShouldThrowInvalidOperationException()
    {
        Class_ShouldImplement_ITileFactory();

        // Arrange
        _tileFactory!.FillDisplays();
        IEnumerable<TileType> tileTypesNotInCenter = Enum.GetValues<TileType>().Where(t => _tileFactory.TableCenter.Tiles.All(dt => dt != t));
        TileType tileTypeNotInCenter = Random.Shared.NextItem(tileTypesNotInCenter);

        Assert.That(() => _tileFactory.TakeTiles(_tileFactory.TableCenter.Id, tileTypeNotInCenter),
            Throws.InvalidOperationException.With.Message.ContainsOne("tile", "tegel"));
    }

    [MonitoredTest]
    public void AddToUsedTiles_ShouldAddTileToUsedTiles()
    {
        Class_ShouldImplement_ITileFactory();

        // Arrange
        TileType tileType = Random.Shared.NextTileType();

        Assert.That(_tileFactory!.UsedTiles, Is.Empty, "Used tiles should be empty before adding a tile");

        // Act
        _tileFactory!.AddToUsedTiles(tileType);

        // Assert
        Assert.That(_tileFactory.UsedTiles, Contains.Item(tileType), "The used tiles should contain the added tile");
    }
}