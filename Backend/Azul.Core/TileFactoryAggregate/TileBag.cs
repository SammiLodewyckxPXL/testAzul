using Azul.Core.TileFactoryAggregate.Contracts;

namespace Azul.Core.TileFactoryAggregate;

/// <inheritdoc cref="ITileBag"/>
internal class TileBag : ITileBag
{
    private List<TileType> _tiles = new();
    public IReadOnlyList<TileType> Tiles => _tiles.AsReadOnly();

    public void AddTiles(int amount, TileType tileType)
    {
        for (int i = 0; i < amount; i++)
        {
            _tiles.Add(tileType);
        }
    }

    public void AddTiles(IReadOnlyList<TileType> tilesToAdd)
    {
        _tiles.AddRange(tilesToAdd);
    }

    public bool TryTakeTiles(int amount, out IReadOnlyList<TileType> tiles)
    {
        List<TileType> takenTiles = new List<TileType>();
        tiles = takenTiles;

        if (amount <= 0 || _tiles.Count == 0)
        {
            return false;
        }

        Random random = new Random();
        for (int i = _tiles.Count - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            TileType temp = _tiles[i];
            _tiles[i] = _tiles[j];
            _tiles[j] = temp;
        }

        int takeCount = Math.Min(amount, _tiles.Count);

        for (int i = 0; i < takeCount; i++)
        {
            takenTiles.Add(_tiles[i]);
        }

        _tiles.RemoveRange(0, takeCount);

        return takeCount == amount;
    }
}