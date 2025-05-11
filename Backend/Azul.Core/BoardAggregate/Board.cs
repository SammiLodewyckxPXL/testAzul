using Azul.Core.BoardAggregate.Contracts;
using Azul.Core.TileFactoryAggregate.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Azul.Core.BoardAggregate;

/// <inheritdoc cref="IBoard"/>
internal class Board : IBoard
{
    public IPatternLine[] PatternLines { get; }
    public TileSpot[,] Wall { get; private set; }
    public TileSpot[] FloorLine { get; }
    public int Score { get; private set; }
    private static readonly TileType[,] wallPattern = new TileType[5, 5]
{
    { TileType.PlainBlue,     TileType.YellowRed,     TileType.PlainRed,      TileType.BlackBlue,    TileType.WhiteTurquoise },
    { TileType.WhiteTurquoise, TileType.PlainBlue,     TileType.YellowRed,     TileType.PlainRed,     TileType.BlackBlue },
    { TileType.BlackBlue,     TileType.WhiteTurquoise, TileType.PlainBlue,     TileType.YellowRed,    TileType.PlainRed },
    { TileType.PlainRed,      TileType.BlackBlue,      TileType.WhiteTurquoise,TileType.PlainBlue,     TileType.YellowRed },
    { TileType.YellowRed,     TileType.PlainRed,       TileType.BlackBlue,     TileType.WhiteTurquoise,TileType.PlainBlue }
};

    public bool HasCompletedHorizontalLine
    {
        get
        {
            for (int row = 0; row < 5; row++)
            {
                if (Wall.GetRow(row).All(spot => spot.HasTile))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public Board()
    {
        PatternLines = new IPatternLine[5];
        for (int i = 0; i < PatternLines.Length; i++)
        {
            PatternLines[i] = new PatternLine(i + 1);
        }

        Wall = new TileSpot[5, 5];


        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col < 5; col++)
            {
                Wall[row, col] = new TileSpot(wallPattern[row, col]);
            }
        }

        FloorLine = new TileSpot[7];
        for (int i = 0; i < FloorLine.Length; i++)
        {
            FloorLine[i] = new TileSpot(null);
        }

        Score = 0;
    }

    public void AddTilesToFloorLine(IReadOnlyList<TileType> tilesToAdd, ITileFactory tileFactory)
    {
        foreach (var tile in tilesToAdd)
        {
            // Zoek een lege plek in de vloerregel
            TileSpot? availableSpot = FloorLine.FirstOrDefault(spot => !spot.HasTile);

            if (availableSpot != null)
            {
                // Probeer de tegel te plaatsen op een lege plek
                availableSpot.PlaceTile(tile);
            }
            else
            {
                // Geen lege plekken meer; voeg de tegel toe aan de gebruikte tegels
                tileFactory.AddToUsedTiles(tile);
            }
        }
    }

    public void AddTilesToPatternLine(IReadOnlyList<TileType> tilesToAdd, int patternLineIndex, ITileFactory tileFactory)
    {
        if (patternLineIndex < 0 || patternLineIndex >= PatternLines.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(patternLineIndex), "Invalid pattern line index.");
        }

        var patternLine = PatternLines[patternLineIndex];
        var tileType = tilesToAdd.FirstOrDefault(t => t != TileType.StartingTile);
        if (tileType == default)
        {
            throw new InvalidOperationException("No valid tile type found to place.");
        }

        TileType currentType = patternLine.NumberOfTiles > 0
            ? patternLine.TileType!.Value
            : tileType;

        int rowIndex = patternLineIndex;
        bool tileExistsInRow = Wall.GetRow(rowIndex).Any(spot => spot.Type == currentType && spot.HasTile);
        if (tileExistsInRow)
        {
            throw new InvalidOperationException("Cannot add tiles: wall (muur) already contains this tile type in the row.");
        }

        var validTiles = tilesToAdd.Where(t => t == currentType).ToList();
        var overflowTiles = tilesToAdd.Where(t => t != currentType).ToList();


        int overflow = validTiles.Count;

        if (!patternLine.IsComplete)
        {
            patternLine.TryAddTiles(currentType, validTiles.Count, out overflow);
        }

        if (overflow > 0)
        {
            overflowTiles.AddRange(validTiles.Skip(validTiles.Count - overflow));
        }

        if (overflowTiles.Count > 0)
        {
            AddTilesToFloorLine(overflowTiles, tileFactory);
        }

    }


    public void CalculateFinalBonusScores()
    {
        // Bonus voor volledige rijen
        for (int row = 0; row < 5; row++)
        {
            if (Wall.GetRow(row).All(spot => spot.HasTile))
            {
                Score += 2;
            }
        }

        // Bonus voor volledige kolommen
        for (int col = 0; col < 5; col++)
        {
            if (Wall.GetColumn(col).All(spot => spot.HasTile))
            {
                Score += 7;
            }
        }

        // Bonus voor volledig voltooide kleuren (alleen als ze op de juiste plekken liggen)
        foreach (TileType type in Enum.GetValues(typeof(TileType)))
        {
            if (type == TileType.StartingTile) continue;//slaagt de startintile over
            bool complete = true;
            for (int row = 0; row < 5 && complete; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    if (wallPattern[row, col] == type && !Wall[row, col].HasTile)
                    {
                        complete = false;
                        break;
                    }
                }
            }

            if (complete)
            {
                Score += 10;
            }
        }
    }

    public void DoWallTiling(ITileFactory tileFactory)
    {
        foreach (var patternLine in PatternLines)
        {
            if (patternLine.IsComplete)
            {
                var tileType = patternLine.TileType;
                int rowIndex = Array.IndexOf(PatternLines, patternLine);
                int colIndex = Array.FindIndex(Wall.GetRow(rowIndex), spot => spot.Type == tileType);

                if (colIndex >= 0 && !Wall[rowIndex, colIndex].HasTile)
                {
                    // Plaats tegel op de muur
                    Wall[rowIndex, colIndex].PlaceTile(tileType!.Value);

                    // Bereken en update de score
                    int scoreForThisTile = CalculateScoreForTile(rowIndex, colIndex);
                    Score += scoreForThisTile;

                    // Verplaats overtollige tegels naar de fabriek
                    int excessTiles = patternLine.NumberOfTiles - 1;
                    if (excessTiles > 0)
                    {
                        Console.WriteLine($"Overflow tiles to factory: {excessTiles}"); // Log overflow
                        for (int i = 0; i < excessTiles; i++)
                        {
                            tileFactory.AddToUsedTiles(tileType.Value);
                        }
                    }
                }

                // Maak patroonlijn leeg
                patternLine.Clear();
            }
        }

        // Verwerk tegels op de vloerlijn en trek strafpunten af
        int[] floorLinePenalties = { 1, 1, 2, 2, 2, 3, 3 }; // Strafpunten per positie in de vloerregel
        for (int i = 0; i < FloorLine.Length; i++)
        {
            var spot = FloorLine[i];
            if (spot.HasTile)
            {
                if (spot.Type != TileType.StartingTile)
                {
                    // Alleen niet-starting tiles toevoegen aan de gebruikte tegels
                    tileFactory.AddToUsedTiles(spot.Type!.Value);
                }
                spot.Clear();
                Score -= floorLinePenalties[i]; // Strafpunten aftrekken
            }
        }
    }

    private int CalculateScoreForTile(int row, int col)
    {
        // Horizontale score: tel aaneengesloten tegels in de rij
        int horizontal = 1; // Start met 1 voor de geplaatste tegel
                            // Naar links
        for (int c = col - 1; c >= 0 && Wall[row, c].HasTile; c--)
        {
            horizontal++;
        }
        // Naar rechts
        for (int c = col + 1; c < 5 && Wall[row, c].HasTile; c++)
        {
            horizontal++;
        }

        // Verticale score: tel aaneengesloten tegels in de kolom
        int vertical = 1; // Start met 1 voor de geplaatste tegel
                          // Naar boven
        for (int r = row - 1; r >= 0 && Wall[r, col].HasTile; r--)
        {
            vertical++;
        }
        // Naar onder
        for (int r = row + 1; r < 5 && Wall[r, col].HasTile; r++)
        {
            vertical++;
        }
        // Bepaal de totale score
        int totalScore = 0;
        if (horizontal > 1) totalScore += horizontal;
        if (vertical > 1) totalScore += vertical;
        if (totalScore == 0) totalScore = 1; // Minimaal 1 punt

        return totalScore;
    }
}

internal static class TileSpotExtensions
{
    public static TileSpot[] GetRow(this TileSpot[,] wall, int rowIndex)
    {
        return Enumerable.Range(0, wall.GetLength(1)).Select(col => wall[rowIndex, col]).ToArray();
    }

    public static TileSpot[] GetColumn(this TileSpot[,] wall, int colIndex)
    {
        return Enumerable.Range(0, wall.GetLength(0)).Select(row => wall[row, colIndex]).ToArray();
    }

    public static IEnumerable<TileSpot> Flatten(this TileSpot[,] wall)
    {
        for (int row = 0; row < wall.GetLength(0); row++)
        {
            for (int col = 0; col < wall.GetLength(1); col++)
            {
                yield return wall[row, col];
            }
        }
    }
}
