using Azul.Core.GameAggregate.Contracts;
using Azul.Core.UserAggregate;

namespace Azul.Core.TableAggregate.Contracts
{
    /// <summary>
    /// Manages all the tables of the application
    /// </summary>
    public interface ITableManager
    {
        /// <summary>
        /// Searches a table with available seats that matches the given preferences.
        /// If such a table is found, the user joins it.
        /// If no table is found, a new table is created and the user joins the new table.
        /// </summary>
        /// <returns>The table the user has joined</returns>
        ITable JoinOrCreateTable(User user, ITablePreferences preferences);

        /// <summary>
        /// Removes a user from a table.
        /// If the table has no players left, it is removed from the system.
        /// </summary>
        void LeaveTable(Guid tableId, User user);

        /// <summary>
        /// Starts a game for a table.
        /// </summary>
        IGame StartGameForTable(Guid tableId);

        /// <summary>
        /// EXTRA: Fills the table with computer players.
        /// </summary>
        /// <remarks>This is an EXTRA. Not needed to implement the minimal requirements.</remarks>
        void FillWithArtificialPlayers(Guid tableId, User user);
    }
}
