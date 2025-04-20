using System.Runtime.CompilerServices;
using Azul.Core.GameAggregate.Contracts;
using Azul.Core.PlayerAggregate.Contracts;
using Azul.Core.TableAggregate.Contracts;
using Azul.Core.UserAggregate;

namespace Azul.Core.TableAggregate;

/// <inheritdoc cref="ITableManager"/>
internal class TableManager : ITableManager
{
    private readonly ITableRepository _tableRepository;
    private readonly ITableFactory _tableFactory;
    private readonly IGameRepository _gameRepository;
    private readonly IGameFactory _gameFactory;
    private readonly IGamePlayStrategy _gamePlayStrategy;

    public TableManager(
        ITableRepository tableRepository,
        ITableFactory tableFactory,
        IGameRepository gameRepository,
        IGameFactory gameFactory,
        IGamePlayStrategy gamePlayStrategy)
    {
        _tableRepository = tableRepository;
        _tableFactory = tableFactory;
        _gameRepository = gameRepository;
        _gameFactory = gameFactory;
        _gamePlayStrategy = gamePlayStrategy;
    }

    public ITable JoinOrCreateTable(User user, ITablePreferences preferences)
    {
        List<ITable> availabletables = (List<ITable>)_tableRepository.FindTablesWithAvailableSeats(preferences);
        if (availabletables.Any())
        {
            ITable table = availabletables.First();
            table.Join(user);
            return table;
        }
        else
        {
            ITable newtable = _tableFactory.CreateNewForUser(user, preferences);
            _tableRepository.Add(newtable);
            return newtable;
        }

            //Find a table with available seats that matches the given preferences
            //If no table is found, create a new table. Otherwise, take the first available table
    }

    public void LeaveTable(Guid tableId, User user)
    {
        ITable table = _tableRepository.Get(tableId);
        if (table.SeatedPlayers.Count > 1)
        {
            table.Leave(user.Id);
        }
        else
        {
            table.Leave(user.Id);
            _tableRepository.Remove(tableId);
        }
    }


    public IGame StartGameForTable(Guid tableId)
    {
        ITable table = _tableRepository.Get(tableId);
        if (table.HasAvailableSeat)
        {
            throw new InvalidOperationException("there are not enough people");
        }
        else
        {
            IGame newgame = _gameFactory.CreateNewForTable(table);
            _gameRepository.Add(newgame);
            table.GameId = newgame.Id;
            return newgame;
        }
    }

    public void FillWithArtificialPlayers(Guid tableId, User user)
    {
        //TODO: Implement this method when you are working on the EXTRA requirement 'Play against AI'
        throw new NotImplementedException();
    }
}