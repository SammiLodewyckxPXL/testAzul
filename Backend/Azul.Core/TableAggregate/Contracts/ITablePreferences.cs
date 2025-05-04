using System.ComponentModel;

namespace Azul.Core.TableAggregate.Contracts;

public interface ITablePreferences
{
    /// <summary>
    /// Total number of players that can take part in the game.
    /// The default value is 2.
    /// </summary>
    [DefaultValue(2)]
    int NumberOfPlayers { get; set; }

    /// <summary>
    /// Number of artificial players that should take part in the game.
    /// The default value is 0.
    /// Must be less than <see cref="NumberOfPlayers"/>.
    /// </summary>
    [DefaultValue(0)]
    int NumberOfArtificialPlayers { get; set; }

    /// <summary>
    /// Number of factory displays that are used in the game.
    /// This value is calculated based on the <see cref="NumberOfPlayers"/>.
    /// </summary>
    int NumberOfFactoryDisplays { get; }
}