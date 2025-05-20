using Azul.Core.TableAggregate.Contracts;
using Azul.Core.TileFactoryAggregate;

namespace Azul.Core.GameAggregate.Contracts;

/// <summary>
/// Factory to create games
/// </summary>
public interface IGameFactory
{
    /// <summary>
    /// Creates a new game for a table.
    /// Steps include:
    /// * Fill a <see cref="TileBag"/> with 20 tiles of each color
    /// * Create a tile factory with displays (amount is taken from <see cref="ITablePreferences"/>) and the created bag
    /// * Generate a game id
    /// * Create a game with the created tile factory and the players that are seated at the table
    /// </summary>
    /// <param name="table">The table that wants to play a game</param>
    IGame CreateNewForTable(ITable table);
}