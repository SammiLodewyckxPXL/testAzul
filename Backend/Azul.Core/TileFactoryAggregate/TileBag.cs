using Azul.Core.TileFactoryAggregate.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Azul.Core.TileFactoryAggregate
{
    internal class TileBag : ITileBag
    {
        private readonly List<TileType> _tiles = new();

        public IReadOnlyList<TileType> Tiles => _tiles.AsReadOnly();

        public void AddTiles(int amount, TileType tileType)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));

            for (int i = 0; i < amount; i++)
            {
                _tiles.Add(tileType);
            }
        }

        public void AddTiles(IReadOnlyList<TileType> tiles)
        {
            _tiles.AddRange(tiles);
        }

        public bool TryTakeTiles(int numberOfTilesToTake, out IReadOnlyList<TileType> takenTiles)
        {
            takenTiles = new List<TileType>();

            if (_tiles.Count < numberOfTilesToTake)
            {
                takenTiles = _tiles.ToList();
                _tiles.Clear();
                return false;
            }

            List<TileType> selected = new();
            for (int i = 0; i < numberOfTilesToTake; i++)
            {
                int index = Random.Shared.Next(_tiles.Count);
                selected.Add(_tiles[index]);
                _tiles.RemoveAt(index);
            }

            takenTiles = selected;
            return true;
        }
    }
}
