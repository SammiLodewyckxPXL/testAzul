using Azul.Core.GameAggregate.Contracts;
using Azul.Core.PlayerAggregate.Contracts;
using Azul.Core.TileFactoryAggregate.Contracts;
using System;
using System.Linq;

namespace Azul.Core.GameAggregate;

internal class Game : IGame
{
    private readonly Guid _id;
    private readonly ITileFactory _tileFactory;
    private readonly IPlayer[] _players;
    private Guid _playerToPlayId;
    private int _roundNumber;
    private bool _hasEnded;

    public Game(Guid id, ITileFactory tileFactory, IPlayer[] players)
    {
        _id = id;
        _tileFactory = tileFactory;
        _players = players;

        _roundNumber = 1;
        _hasEnded = false;

        _playerToPlayId = players
            .OrderByDescending(p => p.LastVisitToPortugal ?? DateOnly.MinValue)
            .First().Id;

        _tileFactory.TableCenter.AddStartingTile();
        _tileFactory.FillDisplays();
    }

    public Guid Id => _id;

    public ITileFactory TileFactory => _tileFactory;

    public IPlayer[] Players => _players;

    public Guid PlayerToPlayId => _playerToPlayId;

    public int RoundNumber => _roundNumber;

    public bool HasEnded => _hasEnded;

    public void TakeTilesFromFactory(Guid playerId, Guid displayId, TileType tileType)
    {
        var player = GetPlayer(playerId);

        if (playerId != _playerToPlayId)
        {
            throw new InvalidOperationException("It's not this player's turn.");
        }

        if (player.TilesToPlace.Count > 0)
        {
            throw new InvalidOperationException("Player already has tiles to place.");
        }

        var tiles = _tileFactory.TakeTiles(displayId, tileType);
        player.TilesToPlace.AddRange(tiles);

        if (tiles.Contains(TileType.StartingTile))
        {
            player.HasStartingTile = true;
        }
    }

    public void PlaceTilesOnPatternLine(Guid playerId, int patternLineIndex)
    {
        var player = GetPlayer(playerId);

        if (playerId != _playerToPlayId)
        {
            throw new InvalidOperationException("It's not this player's turn.");
        }

        if (!player.TilesToPlace.Any())
        {
            throw new InvalidOperationException("Player has no tiles to place.");
        }

        player.Board.AddTilesToPatternLine(player.TilesToPlace, patternLineIndex, _tileFactory);
        player.TilesToPlace.Clear();

        if (_tileFactory.IsEmpty)
        {
            foreach (var p in _players)
            {
                p.Board.DoWallTiling(_tileFactory);
            }

            if (_players.Any(p => p.Board.HasCompletedHorizontalLine))
            {
                foreach (var p in _players)
                {
                    p.Board.CalculateFinalBonusScores();
                }
                _hasEnded = true;
                return;
            }

            _roundNumber++;

            var starter = _players.First(p => p.HasStartingTile);
            _playerToPlayId = starter.Id;

            foreach (var p in _players)
            {
                p.HasStartingTile = false;
            }

            _tileFactory.TableCenter.AddStartingTile();
            _tileFactory.FillDisplays();
        }
        else
        {
            SwitchTurn();
        }
    }

    public void PlaceTilesOnFloorLine(Guid playerId)
    {
        var player = GetPlayer(playerId);

        if (playerId != _playerToPlayId)
        {
            throw new InvalidOperationException("It's not this player's turn.");
        }

        player.Board.AddTilesToFloorLine(player.TilesToPlace, _tileFactory);
        player.TilesToPlace.Clear();
        SwitchTurn();
    }

    private IPlayer GetPlayer(Guid id)
    {
        return _players.First(p => p.Id == id);
    }

    private void SwitchTurn()
    {
        var currentIndex = Array.FindIndex(_players, p => p.Id == _playerToPlayId);
        var nextIndex = (currentIndex + 1) % _players.Length;
        _playerToPlayId = _players[nextIndex].Id;
    }
}
