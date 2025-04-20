using Azul.Core.BoardAggregate.Contracts;
using Azul.Core.TileFactoryAggregate.Contracts;
using System;

namespace Azul.Core.BoardAggregate;

internal class PatternLine : IPatternLine
{
    public int Length { get; }

    public TileType? TileType { get; private set; }

    public int NumberOfTiles { get; private set; }

    public bool IsComplete => NumberOfTiles == Length;

    public PatternLine(int length)
    {
        Length = length;
        TileType = null;
        NumberOfTiles = 0;
    }

    public void Clear()
    {
        TileType = null;
        NumberOfTiles = 0;
    }

    public void TryAddTiles(TileType type, int numberOfTilesToAdd, out int remainingNumberOfTiles)
    {
        if (IsComplete)
        {
            throw new InvalidOperationException("Cannot add tiles to a complete pattern line.");
        }

        if (TileType != null && TileType != type)
        {
            throw new InvalidOperationException("Pattern line already contains a different tile type.");
        }

        TileType ??= type;

        int spaceLeft = Length - NumberOfTiles;
        int tilesToAdd = Math.Min(spaceLeft, numberOfTilesToAdd);
        NumberOfTiles += tilesToAdd;

        remainingNumberOfTiles = numberOfTilesToAdd - tilesToAdd;
    }
}
