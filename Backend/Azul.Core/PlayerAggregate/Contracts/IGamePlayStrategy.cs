using Azul.Core.GameAggregate.Contracts;
using Azul.Core.TileFactoryAggregate.Contracts;

namespace Azul.Core.PlayerAggregate.Contracts;

/// <summary>
/// A strategy for an AI player. The strategy can be used to determine a move for the AI player in a game.
/// </summary>
/// <remarks>This is an EXTRA. Not needed to implement the minimal requirements.</remarks>
public interface IGamePlayStrategy
{
    /// <summary>
    /// Gets the best 'take tiles' move to play for a player.
    /// </summary>
    /// <param name="playerId">Identifier of the (AI) player that wants to make a move.</param>
    /// <param name="game">The game the (AI) player is in.</param>
    /// <returns>
    /// The best move according to the strategy.
    /// </returns>
    ITakeTilesMove GetBestTakeTilesMove(Guid playerId, IGame game);

    /// <summary>
    /// Gets the best 'place tiles' move to play for a player.
    /// </summary>
    /// <param name="playerId">Identifier of the (AI) player that wants to make a move.</param>
    /// <param name="game">The game the (AI) player is in.</param>
    /// <returns>
    /// The best move according to the strategy.
    /// </returns>
    IPlaceTilesMove GetBestPlaceTilesMove(Guid playerId, IGame game);

}