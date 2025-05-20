namespace Azul.Core.TileFactoryAggregate.Contracts;

public interface IFactoryDisplay
{
    public Guid Id { get; }

    public IReadOnlyList<TileType> Tiles { get; }

    public bool IsEmpty { get; }

    public void AddTiles(IReadOnlyList<TileType> tilesToAdd);

    public IReadOnlyList<TileType> TakeTiles(TileType tileType);
}