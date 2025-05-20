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
    private Guid? _startingTilePlayerId;

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
        _tileFactory.FillDisplays();
        _tileFactory.TableCenter.AddStartingTile();
        
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

        if (player.TilesToPlace.Contains(TileType.StartingTile))
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

        RegisterStartingTile(player);


        player.TilesToPlace.Clear();

        if (_tileFactory.IsEmpty && _players.All(p => p.TilesToPlace.Count == 0))
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

            if (_startingTilePlayerId.HasValue)
            {
                _playerToPlayId = _startingTilePlayerId.Value;
                _startingTilePlayerId = null;
            }
            else
            {
                var starter = _players.First(p => p.HasStartingTile);
                _playerToPlayId = starter.Id;
            }

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

        RegisterStartingTile(player);

        player.Board.AddTilesToFloorLine(player.TilesToPlace, _tileFactory);
        player.TilesToPlace.Clear();

        if (_tileFactory.IsEmpty && _players.All(p => p.TilesToPlace.Count == 0))
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

            if (_startingTilePlayerId.HasValue)
            {
                _playerToPlayId = _startingTilePlayerId.Value;
                _startingTilePlayerId = null;
            }
            else
            {
                var starter = _players.First(p => p.HasStartingTile);
                _playerToPlayId = starter.Id;
            }

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

    private IPlayer GetPlayer(Guid id)
    {
        return _players.First(p => p.Id == id);
    }

    private void SwitchTurn()
    {
        var currentIndex = Array.FindIndex(_players, p => p.Id == _playerToPlayId);
        if (currentIndex == -1)
        {
            throw new InvalidOperationException("Current player ID not found in player list.");
        }

        var nextIndex = (currentIndex + 1) % _players.Length;
        Console.WriteLine($"SwitchTurn: From Player {_playerToPlayId} (Index {currentIndex}) to Player {_players[nextIndex].Id} (Index {nextIndex}).");
        _playerToPlayId = _players[nextIndex].Id;
    }

    private void RegisterStartingTile(IPlayer player)
    {
        if (player.TilesToPlace.Contains(TileType.StartingTile) && _startingTilePlayerId == null)
        {
            _startingTilePlayerId = player.Id;
            _tileFactory.AddToUsedTiles(TileType.StartingTile);
        }
    }

}
