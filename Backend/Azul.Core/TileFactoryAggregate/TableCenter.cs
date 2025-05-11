using Azul.Core.TileFactoryAggregate.Contracts;

namespace Azul.Core.TileFactoryAggregate;

internal class TableCenter : ITableCenter
{
    private readonly List<TileType> _tiles = new();

    public Guid Id { get; set; }

    public IReadOnlyList<TileType> Tiles => _tiles.AsReadOnly();

    public bool IsEmpty => _tiles.Count == 0; 

    public void AddStartingTile()
    {
        _tiles.Add(TileType.StartingTile);
    }

    public void AddTiles(IReadOnlyList<TileType> tilesToAdd)
    {
        _tiles.AddRange(tilesToAdd); 
    }

    public IReadOnlyList<TileType> TakeTiles(TileType tileType)
    {
        List<TileType> takenTiles = new List<TileType>();

        for (int i = _tiles.Count - 1; i >= 0; i--)
        {
            if (_tiles[i] == tileType)
            {
                takenTiles.Add(_tiles[i]);
                _tiles.RemoveAt(i);
            }
        }

        takenTiles.Reverse();
        return takenTiles.AsReadOnly();
    }
    public void Clear()
    {
        _tiles.Clear();
    }

}
