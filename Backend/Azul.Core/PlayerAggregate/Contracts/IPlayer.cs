using System.Drawing;
using Azul.Core.BoardAggregate.Contracts;
using Azul.Core.TileFactoryAggregate.Contracts;

namespace Azul.Core.PlayerAggregate.Contracts;

/// <summary>
/// Represents a player in the game.
/// </summary>
public interface IPlayer
{
    /// <summary>
    /// Unique identifier of the player
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// (Display) name of the player
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The date the player last visited Portugal. Null if the player has never visited Portugal.
    /// </summary>
    public DateOnly? LastVisitToPortugal { get; }

    /// <summary>
    /// The board of the player containing the pattern lines, the wall and the floor line
    /// </summary>
    public IBoard Board { get; }

    /// <summary>
    /// Indicates whether the player has a starting tile (taken from the center or given at the start of the game)
    /// </summary>
    public bool HasStartingTile { get; set; }

    /// <summary>
    /// The tiles that the player has taken from the factory and must place on the board
    /// </summary>
    public List<TileType> TilesToPlace { get; }
}