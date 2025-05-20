using Azul.Core.GameAggregate.Contracts;
using Azul.Core.PlayerAggregate;
using Azul.Core.PlayerAggregate.Contracts;
using Azul.Core.TileFactoryAggregate.Contracts;

namespace Azul.Core.GameAggregate;

/// <inheritdoc cref="IGameService"/>
internal class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;
    public GameService(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
    }
    public IGame GetGame(Guid gameId)
    {
        return _gameRepository.GetById(gameId);
    }
    public void TakeTilesFromFactory(Guid gameId, Guid playerId, Guid displayId, TileType tileType)
    {
        var game = _gameRepository.GetById(gameId);
        if (game == null)
        {
            throw new ArgumentException($"Game with ID {gameId} not found.");
        }

        game.TakeTilesFromFactory(playerId, displayId, tileType);
    }

    public void PlaceTilesOnPatternLine(Guid gameId, Guid playerId, int patternLineIndex)
    {
        var game = _gameRepository.GetById(gameId);
        if (game == null)
        {
            throw new ArgumentException($"Game with ID {gameId} not found.");
        }
        game.PlaceTilesOnPatternLine(playerId, patternLineIndex);
    }

    public void PlaceTilesOnFloorLine(Guid gameId, Guid playerId)
    {
        var game = _gameRepository.GetById(gameId);
        if (game == null)
        {
            throw new ArgumentException($"Game with ID {gameId} not found.");
        }
        game.PlaceTilesOnFloorLine(playerId);
    }
}