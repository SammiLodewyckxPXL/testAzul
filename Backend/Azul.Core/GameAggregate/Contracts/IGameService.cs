using Azul.Core.TileFactoryAggregate.Contracts;

namespace Azul.Core.GameAggregate.Contracts;

public interface IGameService
{
    IGame GetGame(Guid gameId);

    /// <summary>
    /// Takes all the tiles of the specified type from the specified display.
    /// </summary>
    /// <param name="gameId">Unique identifier of the game</param>
    /// <param name="playerId">Unique identifier of the player</param>
    /// <param name="displayId">Unique identifier of the factory display (the table center is considered to be a special kind of factory display)</param>
    /// <param name="tileType">The type of the tiles to take</param>
    void TakeTilesFromFactory(Guid gameId, Guid playerId, Guid displayId, TileType tileType);

    /// <summary>
    /// Places the tiles the player has previously taken on a pattern line.
    /// </summary>
    /// <param name="gameId">Unique identifier of the game</param>
    /// <param name="playerId">Unique identifier of the player</param>
    /// <param name="patternLineIndex">The index of the target pattern line (0 is the top line, 4 is the bottom line)</param>
    void PlaceTilesOnPatternLine(Guid gameId, Guid playerId, int patternLineIndex);

    /// <summary>
    /// Places the tiles the player has previously taken on the floor line.
    /// </summary>
    /// <param name="gameId">Unique identifier of the game</param>
    /// <param name="playerId">Unique identifier of the player</param>
    void PlaceTilesOnFloorLine(Guid gameId, Guid playerId);
}