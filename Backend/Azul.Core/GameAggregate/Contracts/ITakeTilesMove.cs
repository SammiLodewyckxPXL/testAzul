using Azul.Core.TileFactoryAggregate.Contracts;

namespace Azul.Core.GameAggregate.Contracts;

/// <summary>
/// Represents a move where a player takes all tiles of a certain type from a factory display.
/// </summary>
/// <remarks>
/// EXTRA: Not needed to implement the minimal requirements.
/// </remarks>
public interface ITakeTilesMove
{
    /// <summary>
    /// The unique identifier of the factory display (can be a normal factory display or can be the table center).
    /// </summary>
    public Guid FactoryDisplayId { get; }
    public TileType TileType { get; }
    public int NumberOfTakenTiles { get; }
}