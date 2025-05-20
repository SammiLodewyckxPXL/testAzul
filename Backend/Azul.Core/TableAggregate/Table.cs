using System.Drawing;
using System.Numerics;
using Azul.Core.PlayerAggregate;
using Azul.Core.PlayerAggregate.Contracts;
using Azul.Core.TableAggregate.Contracts;
using Azul.Core.UserAggregate;

namespace Azul.Core.TableAggregate;

/// <inheritdoc cref="ITable"/>
internal class Table : ITable
{
    private List<IPlayer> _seatedPlayers;
    public Guid Id { get; set; }
    public ITablePreferences Preferences { get; }
    public IReadOnlyList<IPlayer> SeatedPlayers => _seatedPlayers.AsReadOnly();
    public bool HasAvailableSeat { get; set; }
    public Guid GameId { get; set; }

    internal Table(Guid id, ITablePreferences preferences)
    {
        Id = id;
        Preferences = preferences;
        HasAvailableSeat = true;
        _seatedPlayers = new List<IPlayer>();
        GameId = Guid.Empty;
    }

    public void FillWithArtificialPlayers(IGamePlayStrategy gamePlayStrategy)
    {
        throw new NotImplementedException();
    }

    public void Join(User user)
    {
         if (!HasAvailableSeat)
        {
            throw new InvalidOperationException("The table is full. No available seats.");
        }

        foreach (IPlayer p in _seatedPlayers)
        {
            if (p.Id == user.Id)
            {
                throw new InvalidOperationException("User is already seated.");
            }
        }

        IPlayer player = new HumanPlayer(user.Id, user.UserName, user.LastVisitToPortugal);
        _seatedPlayers.Add(player);

        HasAvailableSeat = _seatedPlayers.Count < (Preferences.NumberOfPlayers + Preferences.NumberOfArtificialPlayers);
    }

    public void Leave(Guid userId)
    {
        IPlayer playerToLeave = _seatedPlayers.FirstOrDefault(player => player.Id == userId);
        if (playerToLeave != null)
        {
            _seatedPlayers.Remove(playerToLeave);
            HasAvailableSeat = true;
        }
        else
        {
            throw new InvalidOperationException("User not found in seated players.");
        }
    }
}
