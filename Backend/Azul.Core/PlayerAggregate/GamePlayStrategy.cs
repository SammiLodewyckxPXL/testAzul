using Azul.Core.GameAggregate.Contracts;
using Azul.Core.PlayerAggregate.Contracts;

namespace Azul.Core.PlayerAggregate;

internal class GamePlayStrategy : IGamePlayStrategy
{
    public ITakeTilesMove GetBestTakeTilesMove(Guid playerId, IGame game)
    {
        throw new NotImplementedException();
    }

    public IPlaceTilesMove GetBestPlaceTilesMove(Guid playerId, IGame game)
    {
        throw new NotImplementedException();
    }
}