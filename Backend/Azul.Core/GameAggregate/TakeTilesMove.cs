using Azul.Core.GameAggregate.Contracts;
using Azul.Core.TileFactoryAggregate.Contracts;

namespace Azul.Core.GameAggregate;

/// <inheritdoc cref="ITakeTilesMove"/>
internal class TakeTilesMove : ITakeTilesMove
{
    public Guid FactoryDisplayId { get; }
    public TileType TileType { get; }
    public int NumberOfTakenTiles { get; }

    public TakeTilesMove(IFactoryDisplay factoryDisplay, TileType tileType)
    {
        FactoryDisplayId = factoryDisplay.Id;
        TileType = tileType;
        NumberOfTakenTiles = factoryDisplay.Tiles.Count(t => t == tileType);
    }
}