using Azul.Core.TileFactoryAggregate.Contracts;

namespace Azul.Core.BoardAggregate.Contracts;

/// <summary>
/// Represents a pattern line on the board.
/// </summary>
public interface IPatternLine
{
    /// <summary>
    /// The (maximum) length of the pattern line.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// The type of the tiles that are placed on the pattern line.
    /// If no tiles are placed on the pattern line, the value is null.
    /// </summary>
    public TileType? TileType { get; }

    /// <summary>
    /// The number of tiles of <see cref="TileType"/> that are placed on the pattern line.
    /// </summary>
    public int NumberOfTiles { get; }

    /// <summary>
    /// Indicates whether the pattern line is full.
    /// The pattern line is full when the <see cref="NumberOfTiles"/>> on the pattern line is equal to the <see cref="Length"/>> of the pattern line.
    /// </summary>
    public bool IsComplete { get; }

    /// <summary>
    /// Removes all tiles from the pattern line and sets <see cref="TileType"/> to null.
    /// </summary>
    public void Clear();

    /// <summary>
    /// Tries to add the specified number of tiles of the specified type to the pattern line.
    /// </summary>
    /// <param name="type">The type of the tiles that are added</param>
    /// <param name="numberOfTilesToAdd">Number of tiles to add</param>
    /// <param name="remainingNumberOfTiles">Contains the number of tiles that could not be added because the pattern line is complete</param>
    /// <exception cref="InvalidOperationException">When pattern line is already complete or already contains tiles of another type</exception>
    void TryAddTiles(TileType type, int numberOfTilesToAdd, out int remainingNumberOfTiles);
}