using Azul.Core.BoardAggregate.Contracts;
using Azul.Core.TileFactoryAggregate.Contracts;

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

    public void TryAddTiles(TileType type, int numberOfTilesToAdd, out int remainingTilesToAdd)
    {
        if (IsComplete)
        {
            throw new InvalidOperationException("Cannot add tiles to a complete pattern line.");
        }

        // Check if tile type already exists and handle accordingly
        if (TileType != null && TileType != type)
        {
            throw new InvalidOperationException("Pattern line already contains a different tile type.");
        }

        if (TileType == null)
        {
            TileType = type;
        }

        int spaceLeft = Length - NumberOfTiles;
        int tilesToAdd = Math.Min(spaceLeft, numberOfTilesToAdd);
        NumberOfTiles += tilesToAdd;

        remainingTilesToAdd = numberOfTilesToAdd - tilesToAdd;
    }
}