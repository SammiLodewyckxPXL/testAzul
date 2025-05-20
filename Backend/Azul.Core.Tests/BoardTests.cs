using Azul.Core.BoardAggregate;
using Azul.Core.BoardAggregate.Contracts;
using Azul.Core.PlayerAggregate.Contracts;
using Azul.Core.Tests.Extensions;
using Azul.Core.TileFactoryAggregate.Contracts;
using Guts.Client.Core;
using System;
using Microsoft.Extensions.Primitives;
using Moq;
using Azul.Core.GameAggregate;

namespace Azul.Core.Tests;

[ProjectComponentTestFixture("1TINProject", "Azul", "Board",
    @"Azul.Core\BoardAggregate\Board.cs;
Azul.Core\BoardAggregate\PatternLine.cs;
Azul.Core\BoardAggregate\TileSpot.cs;")]
public class BoardTests
{
    private IBoard? _board;

    [SetUp]
    public void SetUp()
    {
        _board = new Board() as IBoard;
    }

    [MonitoredTest]
    public void Class_ShouldBeInternal_SoThatItCanOnlyBeUsedInTheCoreProject()
    {
        Assert.That(typeof(Board).IsNotPublic, Is.True, "use 'internal class' instead of 'public class'");
    }

    [MonitoredTest]
    public void Class_ShouldImplement_IBoard()
    {
        Assert.That(typeof(Board).IsAssignableTo(typeof(IBoard)), Is.True);
    }

    [MonitoredTest]
    public void IBoard_Interface_ShouldHaveCorrectMembers()
    {
        var type = typeof(IBoard);
        type.AssertInterfaceProperty(nameof(IBoard.PatternLines), shouldHaveGetter: true, shouldHaveSetter: false);
        type.AssertInterfaceProperty(nameof(IBoard.Wall), shouldHaveGetter: true, shouldHaveSetter: false);
        type.AssertInterfaceProperty(nameof(IBoard.FloorLine), shouldHaveGetter: true, shouldHaveSetter: false);
        type.AssertInterfaceProperty(nameof(IBoard.Score), shouldHaveGetter: true, shouldHaveSetter: false);
        type.AssertInterfaceProperty(nameof(IBoard.HasCompletedHorizontalLine), shouldHaveGetter: true, shouldHaveSetter: false);
        type.AssertInterfaceMethod(nameof(IBoard.AddTilesToPatternLine), typeof(void), typeof(IReadOnlyList<TileType>), typeof(int), typeof(ITileFactory));
        type.AssertInterfaceMethod(nameof(IBoard.AddTilesToFloorLine), typeof(void), typeof(IReadOnlyList<TileType>), typeof(ITileFactory));
        type.AssertInterfaceMethod(nameof(IBoard.DoWallTiling), typeof(void), typeof(ITileFactory));
        type.AssertInterfaceMethod(nameof(IBoard.CalculateFinalBonusScores), typeof(void));
    }

    [MonitoredTest]
    public void Constructor_ShouldInitializeProperties()
    {
        Assert.That(_board, Is.Not.Null, "Board should implement IBoard");

        //Pattern lines
        Assert.That(_board!.PatternLines, Is.Not.Null, "PatternLines should be initialized");
        Assert.That(_board.PatternLines.Count, Is.EqualTo(5), "There should be 5 pattern lines");
        for (int lineLength = 1; lineLength <= 5; lineLength++)
        {
            IPatternLine? line = _board.PatternLines.FirstOrDefault(pl => pl.Length == lineLength);
            Assert.That(line, Is.Not.Null, $"PatternLine with length {lineLength} is missing");
            Assert.That(line!.NumberOfTiles, Is.Zero, $"PatternLine with length {lineLength} should have zero tiles");
            Assert.That(line!.TileType, Is.Null, $"PatternLine with length {lineLength} should have no tile type set yet");
        }

        //Wall
        Assert.That(_board.Wall, Is.Not.Null, "Wall should be initialized");
        Assert.That(_board.Wall.GetLength(0), Is.EqualTo(5), "Wall should have 5 rows");
        Assert.That(_board.Wall.GetLength(1), Is.EqualTo(5), "Wall should have 5 columns");
        Assert.That(_board.Wall[0, 0].Type, Is.EqualTo(TileType.PlainBlue), "Wall spot (0,0) should be for a plain blue tile");
        Assert.That(_board.Wall[1, 0].Type, Is.EqualTo(TileType.WhiteTurquoise), "Wall spot (1,0) should be for a white turquoise tile");
        Assert.That(_board.Wall[2, 0].Type, Is.EqualTo(TileType.BlackBlue), "Wall spot (2,0) should be for a black blue tile");
        Assert.That(_board.Wall[3, 0].Type, Is.EqualTo(TileType.PlainRed), "Wall spot (3,0) should be for a plain red tile");
        Assert.That(_board.Wall[4, 0].Type, Is.EqualTo(TileType.YellowRed), "Wall spot (4,0) should be for a yellow red tile");
        for (int rowIndex = 0; rowIndex < 5; rowIndex++)
        {
            TileSpot[] row = GetWallRow(rowIndex);
            Assert.That(row.All(ts => ts is not null), Is.True, $"The wall should have no null values");
            Assert.That(row.All(ts => ts.HasTile), Is.False, $"The wall should have no tiles set yet");
            Assert.That(row.Distinct().Count(), Is.EqualTo(5), $"Row {rowIndex} should have 5 different tile types");
        }
        for (int colIndex = 0; colIndex < 5; colIndex++)
        {
            TileSpot[] column = GetWallColumn(colIndex);
            Assert.That(column.Distinct().Count(), Is.EqualTo(5), $"Column {colIndex} should have 5 different tile types");
        }

        //Floor line
        Assert.That(_board.FloorLine, Is.Not.Null, "FloorLine should be initialized");
        Assert.That(_board.FloorLine.Length, Is.EqualTo(7), "FloorLine should have 7 spots");
        Assert.That(_board.FloorLine.All(ts => ts is not null), Is.True, "The floor line should have no null values");
        Assert.That(_board.FloorLine.All(ts => ts.HasTile), Is.False, "The floor line should have no tiles set yet");

        //Score
        Assert.That(_board.Score, Is.Zero, "Score should be initialized to 0");
    }

    [MonitoredTest]
    [TestCase(0, 0, "PlainRed", 1, "")]
    [TestCase(4, 2, "BlackBlue,BlackBlue,BlackBlue", 5, "")]
    [TestCase(2, 0, "StartingTile,YellowRed,YellowRed", 2, "StartingTile")]
    [TestCase(1, 1, "StartingTile,PlainRed,PlainRed", 2, "StartingTile,PlainRed")]
    public void AddTilesToPatternLine_ShouldAddTilesToTheCorrectPatternLine_ShouldAddOverflowToTheFloorline(
        int patternLineIndex,
        int numberOfTilesBeforeAdding,
        string tilesAsText,
        int expectedNumberOfTiles,
        string expectedOverflowAsText)
    {
        Assert.That(_board, Is.Not.Null, "Board should implement IBoard");

        //Arrange
        var tileFactoryMock = new Mock<ITileFactory>();
        List<TileType> tileTypes = tilesAsText.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(Enum.Parse<TileType>).ToList();
        List<TileType> expectedOverflow = expectedOverflowAsText.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(Enum.Parse<TileType>).ToList();

        TileType expectedLineTileType = tileTypes.Last();
        IPatternLine patternLine = _board!.PatternLines[patternLineIndex];
        if (numberOfTilesBeforeAdding > 0)
        {
            patternLine.TryAddTiles(expectedLineTileType, numberOfTilesBeforeAdding, out int remainingNumberOfTiles);
        }

        //Act
        _board!.AddTilesToPatternLine(tileTypes, patternLineIndex, tileFactoryMock.Object);

        //Assert

        Assert.That(patternLine.NumberOfTiles, Is.EqualTo(expectedNumberOfTiles), "The number of tiles in the pattern line is not correct");
        Assert.That(patternLine.TileType, Is.EqualTo(expectedLineTileType), "The tile type of the pattern line is not correct");
        for (int i = 0; i < expectedOverflow.Count; i++)
        {
            Assert.That(_board.FloorLine[i].HasTile && _board.FloorLine[i].Type == expectedOverflow[i], Is.True,
                $"The floor line should have a tile of type '{expectedOverflow[i]}' at index '{i}'");
        }

        tileFactoryMock.Verify(f => f.AddToUsedTiles(It.IsAny<TileType>()), Times.Never,
            "No tiles should have been moved to the used tiles in the tile factory");
    }

    [MonitoredTest]
    public void AddTilesToPatternLine_WallAlreadyHasATileOfThatTypeInTheMatchingRow_ShouldThrowInvalidOperationException()
    {
        Assert.That(_board, Is.Not.Null, "Board should implement IBoard");

        //Arrange
        var tileFactoryMock = new Mock<ITileFactory>();
        int patternLineIndex = Random.Shared.Next(0, 5);
        TileSpot[] matchingWallRow = GetWallRow(patternLineIndex);
        TileType tileType = Random.Shared.NextTileType();
        TileSpot matchingSpot = matchingWallRow.First(ts => ts.Type == tileType);
        matchingSpot.PlaceTile(tileType);

        //Act + Assert
        Assert.That(() => _board!.AddTilesToPatternLine(new List<TileType> { tileType }, patternLineIndex, tileFactoryMock.Object),
            Throws.InvalidOperationException.With.Message.ContainsOne("wall", "muur"));
    }

    [MonitoredTest]
    public void AddTilesToPatternLine_FloorlineIsFull_ShouldAddOverflowToUsedTilesOfFactory()
    {
        Assert.That(_board, Is.Not.Null, "Board should implement IBoard");

        //Arrange
        var tileFactoryMock = new Mock<ITileFactory>();
        TileType tileType = Random.Shared.NextTileType();
        for (int i = 0; i < 7; i++)
        {
            _board!.FloorLine[i].PlaceTile(tileType);
        }

        var tilesToAdd = new List<TileType> { tileType, tileType, tileType };

        //Act
        _board!.AddTilesToPatternLine(tilesToAdd, 0, tileFactoryMock.Object); //1 tile goes to the pattern line, the rest to the floor line

        //Assert
        tileFactoryMock.Verify(f => f.AddToUsedTiles(tileType), Times.Exactly(tilesToAdd.Count - 1),
            "The 'AddToUsedTiles' method of the factory is not called correctly");
    }

    [MonitoredTest]
    public void AddTilesToFloorLine_FloorlineIsAlmostFull_ShouldAddOverflowToUsedTilesOfFactory()
    {
        Assert.That(_board, Is.Not.Null, "Board should implement IBoard");

        //Arrange
        var tileFactoryMock = new Mock<ITileFactory>();
        TileType tileType = Random.Shared.NextTileType();
        //Fill the floor line except for the last spot
        for (int i = 0; i < 6; i++)
        {
            _board!.FloorLine[i].PlaceTile(tileType);
        }

        var tilesToAdd = new List<TileType> { tileType, tileType, tileType };

        //Act
        _board!.AddTilesToFloorLine(tilesToAdd, tileFactoryMock.Object);

        //Assert
        Assert.That(_board!.FloorLine.All(ts => ts.HasTile), Is.True, "The floor line should be full");
        tileFactoryMock.Verify(f => f.AddToUsedTiles(tileType), Times.Exactly(tilesToAdd.Count - 1),
            "The 'AddToUsedTiles' method of the factory is not called correctly");

    }

    [MonitoredTest]
    [TestCase(
        "PlainRed,1\n" +
        "PlainRed,2\n" +
        "PlainRed,3\n" +
        "PlainRed,4\n" +
        "PlainRed,5",
        "0,0,0,0,0\n" +
        "0,0,0,0,0\n" +
        "0,0,0,0,0\n" +
        "0,0,0,0,0\n" +
        "0,0,0,0,0",
        "",
        "Empty,0\n" +
        "Empty,0\n" +
        "Empty,0\n" +
        "Empty,0\n" +
        "Empty,0",
        "0,0,1,0,0\n" +
        "0,0,0,1,0\n" +
        "0,0,0,0,1\n" +
        "1,0,0,0,0\n" +
        "0,1,0,0,0",
        10,
        5)]
    [TestCase(
        "Empty,0\n" +
        "YellowRed,2\n" +
        "PlainBlue,2\n" +
        "Empty,0\n" +
        "WhiteTurquoise,3",
        "0,0,1,0,0\n" +
        "0,0,0,0,0\n" +
        "0,0,1,0,0\n" +
        "0,0,0,0,0\n" +
        "0,0,0,0,0",
        "",
        "Empty,0\n" +
        "Empty,0\n" +
        "PlainBlue,2\n" +
        "Empty,0\n" +
        "WhiteTurquoise,3",
        "0,0,1,0,0\n" +
        "0,0,1,0,0\n" +
        "0,0,1,0,0\n" +
        "0,0,0,0,0\n" +
        "0,0,0,0,0",
        1,
        3)]
    [TestCase(
        "Empty,1\n" +
        "YellowRed,2\n" +
        "PlainBlue,2\n" +
        "Empty,4\n" +
        "WhiteTurquoise,3",
        "0,0,0,0,0\n" +
        "0,1,0,1,1\n" +
        "0,0,0,0,0\n" +
        "0,0,0,0,0\n" +
        "0,0,0,0,0",
        "",
        "Empty,0\n" +
        "Empty,0\n" +
        "PlainBlue,2\n" +
        "Empty,0\n" +
        "WhiteTurquoise,3",
        "0,0,0,0,0\n" +
        "0,1,1,1,1\n" +
        "0,0,0,0,0\n" +
        "0,0,0,0,0\n" +
        "0,0,0,0,0",
        1,
        4)]
    [TestCase(
        "Empty,1\n" +
        "PlainBlue,2\n" +
        "PlainBlue,2\n" +
        "PlainRed,1\n" +
        "Empty,0",
        "0,1,0,0,0\n" +
        "0,0,1,1,1\n" +
        "0,1,0,0,0\n" +
        "0,1,0,0,0\n" +
        "0,1,0,0,0",
        "",
        "Empty,0\n" +
        "Empty,0\n" +
        "PlainBlue,2\n" +
        "PlainRed,1\n" +
        "Empty,0",
        "0,1,0,0,0\n" +
        "0,1,1,1,1\n" +
        "0,1,0,0,0\n" +
        "0,1,0,0,0\n" +
        "0,1,0,0,0",
        1,
        9)]
    [TestCase(
        "Empty,1\n" +
        "PlainBlue,2\n" +
        "PlainBlue,2\n" +
        "PlainRed,1\n" +
        "Empty,0",
        "0,1,0,0,0\n" +
        "1,0,1,1,1\n" +
        "0,1,0,0,0\n" +
        "0,1,0,0,0\n" +
        "0,1,0,0,0",
        "StartingTile, PlainRed, PlainBlue, PlainRed, BlackBlue",
        "Empty,0\n" +
        "Empty,0\n" +
        "PlainBlue,2\n" +
        "PlainRed,1\n" +
        "Empty,0",
        "0,1,0,0,0\n" +
        "1,1,1,1,1\n" +
        "0,1,0,0,0\n" +
        "0,1,0,0,0\n" +
        "0,1,0,0,0",
        1 + 4,
        10 - 8)]
    public void DoWallTiling_ShouldOneTileOfCompletedPatternLinesToWall_ShouldMoveExcessTilesToUsedTilesOfFactory_ShouldCalculateScoreCorrectly(
        string patternLinesAsText, 
        string wallStartingSituationAsText,
        string floorLineAsText,
        string expectedPatternLinesAsText,
        string expectedWallAsText,
        int expectedExcessTiles,
        int expectedScore)
    {
        Assert.That(_board, Is.Not.Null, "Board should implement IBoard");

        //Arrange
        TestContext.Out.WriteLine("PatternLines before wall tiling:");
        TestContext.Out.WriteLine(patternLinesAsText);
        TestContext.Out.WriteLine("-----");
        TestContext.Out.WriteLine("Wall before tiling (1 = has tile):");
        TestContext.Out.WriteLine(wallStartingSituationAsText);
        TestContext.Out.WriteLine("-----");
        TestContext.Out.WriteLine("Floor line before tiling:");
        TestContext.Out.WriteLine(floorLineAsText);
        TestContext.Out.WriteLine("-----");
        TestContext.Out.WriteLine("Expected patternLines after wall tiling:");
        TestContext.Out.WriteLine(expectedPatternLinesAsText);
        TestContext.Out.WriteLine("-----");
        TestContext.Out.WriteLine("Expected wall after tiling (1 = has tile):");
        TestContext.Out.WriteLine(expectedWallAsText);
        TestContext.Out.WriteLine("-----");
        TestContext.Out.WriteLine($"Expected score = {expectedScore}. Expected number of used tiles moved to factory = {expectedExcessTiles}");
        TestContext.Out.WriteLine("-----");

        ArrangePatterLines(patternLinesAsText);
        ArrangeWall(wallStartingSituationAsText);
        ArrangeFloorLine(floorLineAsText);
        var tileFactoryMock = new Mock<ITileFactory>();

        //Act
        _board!.DoWallTiling(tileFactoryMock.Object);

        //Assert
        AssertPatternLines(expectedPatternLinesAsText);
        AssertWall(expectedWallAsText);
        Assert.That(_board!.Score, Is.EqualTo(expectedScore), "The score is not correct");

        tileFactoryMock.Verify(f => f.AddToUsedTiles(It.IsAny<TileType>()), Times.Exactly(expectedExcessTiles),
            $"The 'AddToUsedTiles' method of the factory should be called {expectedExcessTiles} times. " +
            "(Tiles of completed lines that were not moved to the wall, floor line tiles (except starting tile");
        Assert.That(_board!.FloorLine.All(ts => ts.HasTile), Is.False, "The floor line should be empty after wall tiling");
    }

    [MonitoredTest]
    [TestCase(
        "1,0,0,0,0\n" +
        "0,1,1,0,0\n" +
        "1,1,1,1,0\n" +
        "0,0,0,0,0\n" +
        "1,0,0,0,1",
        false)]
    [TestCase(
        "0,0,0,0,0\n" +
        "0,0,1,0,0\n" +
        "1,1,1,1,1\n" +
        "0,0,1,0,0\n" +
        "0,0,0,0,0",
        true)]
    public void HasCompletedHorizontalLine_ShouldReturnTrueWhenOneOfTheWallRowsHasAllTiles(string wallAsText, bool expected)
    {
        Assert.That(_board, Is.Not.Null, "Board should implement IBoard");

        //Arrange
        TestContext.Out.WriteLine("Wall:");
        TestContext.Out.WriteLine(wallAsText);
        TestContext.Out.WriteLine("-----");
        ArrangeWall(wallAsText);

        //Act
        bool hasCompletedHorizontalLine = _board!.HasCompletedHorizontalLine;

        //Assert
        Assert.That(hasCompletedHorizontalLine, Is.EqualTo(expected));
    }

    [MonitoredTest]
    [TestCase(
        "0 horizontal lines, 0 vertical lines, 0 colors",
        "1,1,0,1,1\n" +
        "0,1,1,0,0\n" +
        "1,1,0,1,1\n" +
        "0,0,0,0,0\n" +
        "1,0,0,0,1",
        0)]
    [TestCase(
        "2 horizontal lines, 0 vertical lines, 0 colors", 
        "1,1,1,1,1\n" +
        "0,1,1,0,0\n" +
        "1,1,1,1,1\n" +
        "0,0,0,0,0\n" +
        "1,0,0,0,1",
        4)]
    [TestCase(
        "0 horizontal lines, 3 vertical lines, 0 colors",
        "0,1,1,0,1\n" +
        "0,1,1,0,1\n" +
        "1,1,1,0,1\n" +
        "0,1,1,0,1\n" +
        "0,1,1,0,1",
        21)]
    [TestCase(
        "0 horizontal lines, 0 vertical lines, 1 colors",
        "0,0,1,0,1\n" +
        "1,0,0,1,1\n" +
        "0,0,0,0,1\n" +
        "1,0,1,0,0\n" +
        "0,1,0,0,1",
        10)]
    [TestCase(
        "1 horizontal lines, 1 vertical lines, 1 colors",
        "1,0,0,0,0\n" +
        "1,1,1,1,1\n" +
        "1,0,1,0,1\n" +
        "1,0,0,1,0\n" +
        "1,0,0,0,1",
        19)]
    [TestCase(
        "5 horizontal lines, 5 vertical lines, 5 colors",
        "1,1,1,1,1\n" +
        "1,1,1,1,1\n" +
        "1,1,1,1,1\n" +
        "1,1,1,1,1\n" +
        "1,1,1,1,1",
        95)]
    public void CalculateFinalBonusScores_ShouldSetCorrectScore(string testSituation, string wallAsText, int expectedScore)
    {
        Assert.That(_board, Is.Not.Null, "Board should implement IBoard");

        //Arrange
        TestContext.Out.WriteLine(testSituation);
        TestContext.Out.WriteLine($"Expected bonus score: {expectedScore}");
        TestContext.Out.WriteLine("-----");
        TestContext.Out.WriteLine("Wall:");
        TestContext.Out.WriteLine(wallAsText);
        TestContext.Out.WriteLine("-----");

        ArrangeWall(wallAsText);

        //Act
        _board!.CalculateFinalBonusScores();

        //Assert
        Assert.That(_board.Score, Is.EqualTo(expectedScore));
    }

    private void ArrangePatterLines(string patternLinesAsText)
    {
        string[] lines = patternLinesAsText.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(',');
            if (Enum.TryParse<TileType>(parts[0], out TileType tileType))
            {
                int numberOfTiles = int.Parse(parts[1]);
                _board!.PatternLines[i].TryAddTiles(tileType, numberOfTiles, out _);
            }
        }
    }

    private void ArrangeFloorLine(string floorLineAsText)
    {
        string[] tileTypes = floorLineAsText.Split(',');
        for (int i = 0; i < tileTypes.Length; i++)
        {
            if (Enum.TryParse<TileType>(tileTypes[i], out TileType tileType))
            {
                _board!.FloorLine[i].PlaceTile(tileType);
            }
        }
    }

    private void ArrangeWall(string wallAsText)
    {
        string[] rows = wallAsText.Split('\n');
        for (int i = 0; i < rows.Length; i++)
        {
            string[] cells = rows[i].Split(',');
            for (int j = 0; j < cells.Length; j++)
            {
                if (cells[j] != "0")
                {
                    TileType tileType = _board!.Wall[i, j].Type!.Value;
                    _board!.Wall[i, j].PlaceTile(tileType);
                }
            }
        }
    }

    private void AssertPatternLines(string expectedPatternLinesAsText)
    {
        string[] lines = expectedPatternLinesAsText.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(',');
            if (Enum.TryParse<TileType>(parts[0], out TileType tileType))
            {
                int numberOfTiles = int.Parse(parts[1]);
                Assert.That(_board!.PatternLines[i].NumberOfTiles, Is.EqualTo(numberOfTiles),
                    $"Pattern line with index '{i}' should have {numberOfTiles} tiles after wall tiling");
                Assert.That(_board!.PatternLines[i].TileType, Is.EqualTo(tileType),
                    $"Pattern line with index '{i}' should have tile type {tileType} after wall tiling");
            }
            else
            {
                //Empty
                Assert.That(_board!.PatternLines[i].NumberOfTiles, Is.Zero, $"Pattern line with index '{i}' should be empty after wall tiling");
                Assert.That(_board!.PatternLines[i].TileType, Is.Null, $"Pattern line with index '{i}' should have no tile type after wall tiling");
            }
        }
    }

    private void AssertWall(string expectedWallAsText)
    {
        string[] rows = expectedWallAsText.Split('\n');
        for (int i = 0; i < rows.Length; i++)
        {
            string[] cells = rows[i].Split(',');
            for (int j = 0; j < cells.Length; j++)
            {
                if (cells[j] != "0")
                {
                    Assert.That(_board!.Wall[i, j].HasTile, Is.True, $"Wall cell ({i},{j}) should have a tile after wall tiling");
                }
                else
                {
                    Assert.That(_board!.Wall[i, j].HasTile, Is.False, $"Wall cell ({i},{j}) should not have a tile after wall tiling");
                }
            }
        }
    }

    private TileSpot[] GetWallRow(int rowIndex)
    {
        int columns = _board!.Wall.GetLength(1);
        TileSpot[] row = new TileSpot[columns];
        for (int col = 0; col < columns; col++)
        {
            row[col] = _board.Wall[rowIndex, col];
        }
        return row;
    }

    private TileSpot[] GetWallColumn(int columnIndex)
    {
        int rows = _board!.Wall.GetLength(0);
        TileSpot[] column = new TileSpot[rows];
        for (int row = 0; row < rows; row++)
        {
            column[row] = _board.Wall[row, columnIndex];
        }
        return column;
    }
}