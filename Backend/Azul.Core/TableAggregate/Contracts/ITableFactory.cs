using Azul.Core.UserAggregate;

namespace Azul.Core.TableAggregate.Contracts;

/// <summary>
/// Used to create new tables.
/// </summary>
public interface ITableFactory
{
    /// <summary>
    /// Creates a new table with the given preferences.
    /// The user automatically joins the table.
    /// </summary>
    /// <param name="user">The user</param>
    /// <param name="preferences">Determine the game options (e.g. number of players, ...)</param>
    ITable CreateNewForUser(User user, ITablePreferences preferences);
}