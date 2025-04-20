using Azul.Core.BoardAggregate.Contracts;
using Azul.Core.TileFactoryAggregate.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Azul.Core.BoardAggregate;

internal class Board : IBoard
{
    public IPatternLine[] PatternLines { get; }

    public TileSpot[,] Wall { get; }

    public TileSpot[] FloorLine { get; }

    public int Score { get; private set; }

    public bool HasCompletedHorizontalLine =>
        Enumerable.Range(0, 5).Any(row =>
            Enumerable.Range(0, 5).All(col => Wall[row, col].HasTile));

    public Board()
    {
        PatternLines = new IPatternLine[5];
        for (int i = 0; i < 5; i++)
        {
            PatternLines[i] = new PatternLine(i + 1);
        }

        Wall = new TileSpot[5, 5];
        TileType[,] wallTemplate = new TileType[5, 5]
        {
            { TileType.PlainBlue,    TileType.YellowRed,    TileType.PlainRed,     TileType.BlackBlue,     TileType.WhiteTurquoise },
            { TileType.WhiteTurquoise, TileType.PlainBlue,    TileType.YellowRed,    TileType.PlainRed,       TileType.BlackBlue },
            { TileType.BlackBlue,    TileType.WhiteTurquoise, TileType.PlainBlue,    TileType.YellowRed,      TileType.PlainRed },
            { TileType.PlainRed,     TileType.BlackBlue,     TileType.WhiteTurquoise, TileType.PlainBlue,      TileType.YellowRed },
            { TileType.YellowRed,    TileType.PlainRed,      TileType.BlackBlue,     TileType.WhiteTurquoise, TileType.PlainBlue }
        };

        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col < 5; col++)
            {
                Wall[row, col] = new TileSpot(wallTemplate[row, col]);
            }
        }

        FloorLine = new TileSpot[7];
        for (int i = 0; i < 7; i++)
        {
            FloorLine[i] = new TileSpot();
        }

        Score = 0;
    }

    public void AddTilesToPatternLine(IReadOnlyList<TileType> tilesToAdd, int patternLineIndex, ITileFactory tileFactory)
    {
        throw new NotImplementedException();
    }

    public void AddTilesToFloorLine(IReadOnlyList<TileType> tilesToAdd, ITileFactory tileFactory)
    {
        throw new NotImplementedException();
    }

    public void DoWallTiling(ITileFactory tileFactory)
    {
        throw new NotImplementedException();
    }

    public void CalculateFinalBonusScores()
    {
        throw new NotImplementedException();
    }
}
