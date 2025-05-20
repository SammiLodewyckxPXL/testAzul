namespace Azul.Core.TileFactoryAggregate.Contracts;

public interface ITileFactory
{
    public ITileBag Bag { get; }
    public IReadOnlyList<IFactoryDisplay> Displays { get; }
    public ITableCenter TableCenter { get; }
    public IReadOnlyList<TileType> UsedTiles { get; }
    public bool IsEmpty { get; }
    public void FillDisplays();
    public IReadOnlyList<TileType> TakeTiles(Guid displayId, TileType tileType);
    public void AddToUsedTiles(TileType tile);
}