namespace Azul.Core.GameAggregate.Contracts;

/// <summary>
/// Represents a move where a player places tiles on a pattern line or on the floor line.
/// </summary>
/// <remarks>
/// EXTRA: Not needed to implement the minimal requirements.
/// </remarks>
public interface IPlaceTilesMove
{

    /// <summary>
    /// Indicates whether the tiles should be placed on the floor line.
    /// </summary>
    public bool PlaceInFloorLine { get; }

    /// <summary>
    /// The index of the pattern line where the tiles should be placed.
    /// This value is only relevant if <see cref="PlaceInFloorLine"/> is false.
    /// </summary>
    public int PatternLineIndex { get; }
}