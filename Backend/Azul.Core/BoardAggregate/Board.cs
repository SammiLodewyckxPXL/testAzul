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
        TileType[,] wallPattern = {
         { TileType.PlainBlue, TileType.WhiteTurquoise, TileType.BlackBlue, TileType.PlainRed, TileType.YellowRed },
         { TileType.WhiteTurquoise, TileType.BlackBlue, TileType.PlainRed, TileType.YellowRed, TileType.PlainBlue },
         { TileType.BlackBlue, TileType.PlainRed, TileType.YellowRed, TileType.PlainBlue, TileType.WhiteTurquoise },
         { TileType.PlainRed, TileType.YellowRed, TileType.PlainBlue, TileType.WhiteTurquoise, TileType.BlackBlue },
         { TileType.YellowRed, TileType.PlainBlue, TileType.WhiteTurquoise, TileType.BlackBlue, TileType.PlainRed },
         };

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


    //public void CalculateFinalBonusScores()
    //{
    //    for (int row = 0; row < 5; row++)
    //    {
    //        if (Wall.GetRow(row).All(spot => spot.HasTile))
    //        {
    //            Score += 2;
    //        }
    //    }

    //    for (int col = 0; col < 5; col++)
    //    {
    //        if (Wall.GetColumn(col).All(spot => spot.HasTile))
    //        {
    //            Score += 7;
    //        }
    //    }

    //    foreach (TileType type in Enum.GetValues(typeof(TileType)))
    //    {
    //        if (Wall.Flatten().Where(spot => spot.Type == type).All(spot => spot.HasTile))
    //        {
    //            Score += 10;
    //        }
    //    }
    //}

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

        // Bonus voor volledig voltooide kleuren (op exact de juiste plekken!)
        TileType[,] wallPattern = {
        { TileType.PlainBlue, TileType.WhiteTurquoise, TileType.BlackBlue, TileType.PlainRed, TileType.YellowRed },
        { TileType.WhiteTurquoise, TileType.BlackBlue, TileType.PlainRed, TileType.YellowRed, TileType.PlainBlue },
        { TileType.BlackBlue, TileType.PlainRed, TileType.YellowRed, TileType.PlainBlue, TileType.WhiteTurquoise },
        { TileType.PlainRed, TileType.YellowRed, TileType.PlainBlue, TileType.WhiteTurquoise, TileType.BlackBlue },
        { TileType.YellowRed, TileType.PlainBlue, TileType.WhiteTurquoise, TileType.BlackBlue, TileType.PlainRed },
    };
        foreach (TileType type in Enum.GetValues(typeof(TileType)))
        {
            bool completeColor = true;

            // Loop door het muurpatroon
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    // Als dit het patroon is voor de huidige kleur
                    if (wallPattern[row, col] == type)
                    {
                        // Controleer of er een tegel op de juiste plek ligt
                        if (!Wall[row, col].HasTile)
                        {
                            completeColor = false;
                            break; // We stoppen hier als de kleur niet op de juiste plek ligt
                        }
                    }
                }

                // Als een kleur niet compleet is, breek dan uit de outer loop
                if (!completeColor) break;
            }

            // Als het patroon van de kleur volledig is, voeg dan de bonus toe
            if (completeColor)
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
                if (!Wall[rowIndex, colIndex].HasTile)
                {
                    Wall[rowIndex, colIndex].PlaceTile(tileType!.Value);
                    Score += CalculateScoreForTile(rowIndex, colIndex);
                }

                //Score += CalculateScoreForTile(rowIndex, colIndex);

                patternLine.Clear();
            }
        }

        foreach (var spot in FloorLine)
        {
            if (spot.HasTile)
            {
                tileFactory.AddToUsedTiles(spot.Type!.Value);
                spot.Clear();
            }
        }
    }

    private int CalculateScoreForTile(int row, int col)
    {
        int score = 1;

        // Check row connections
        int left = col - 1;
        while (left >= 0 && Wall[row, left].HasTile)
        {
            score++;
            left--;
        }

        int right = col + 1;
        while (right < 5 && Wall[row, right].HasTile)
        {
            score++;
            right++;
        }

        // Check column connections
        int up = row - 1;
        while (up >= 0 && Wall[up, col].HasTile)
        {
            score++;
            up--;
        }

        int down = row + 1;
        while (down < 5 && Wall[down, col].HasTile)
        {
            score++;
            down++;
        }

        return score;
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
