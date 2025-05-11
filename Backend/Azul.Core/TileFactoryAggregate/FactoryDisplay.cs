using Azul.Core.TileFactoryAggregate.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Azul.Core.TileFactoryAggregate
{
    internal class FactoryDisplay : IFactoryDisplay
    {
        private readonly List<TileType> _tiles = new();
        private readonly ITableCenter _tableCenter;

        public Guid Id { get; } = Guid.NewGuid();

        public IReadOnlyList<TileType> Tiles => _tiles.AsReadOnly();

        public bool IsEmpty => !_tiles.Any();

        public FactoryDisplay(ITableCenter tableCenter)
        {
            _tableCenter = tableCenter;
        }

        public void AddTiles(IReadOnlyList<TileType> tilesToAdd)
        {
            if (tilesToAdd.Count + _tiles.Count <= 4)
            {
                _tiles.AddRange(tilesToAdd);
            }
        }

        public IReadOnlyList<TileType> TakeTiles(TileType tileType)
        {
            if (!_tiles.Contains(tileType))
            {
                throw new InvalidOperationException("The requested tile type is not present in the display.");
            }

            List<TileType> takenTiles = new List<TileType>();
            List<TileType> toMoveToCenter = new List<TileType>();

            foreach (TileType t in _tiles)
            {
                if (t == tileType)
                {
                    takenTiles.Add(t);
                }
                else
                {
                    toMoveToCenter.Add(t);
                }
            }

            _tiles.Clear();
            _tableCenter.AddTiles(toMoveToCenter);

            return takenTiles;
        }
        

        public void Clear()
        {
            _tiles.Clear();
        }
    }
}
