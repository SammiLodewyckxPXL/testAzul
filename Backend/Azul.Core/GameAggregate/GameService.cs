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
        _gameRepository = gameRepository;
    }

    public IGame GetGame(Guid gameId)
    {
        return _gameRepository.GetById(gameId);
    }

    public void TakeTilesFromFactory(Guid gameId, Guid playerId, Guid displayId, TileType tileType)
    {
        throw new NotImplementedException();
    }

    public void PlaceTilesOnPatternLine(Guid gameId, Guid playerId, int patternLineIndex)
    {
        throw new NotImplementedException();
    }

    public void PlaceTilesOnFloorLine(Guid gameId, Guid playerId)
    {
        throw new NotImplementedException();
    }
}
