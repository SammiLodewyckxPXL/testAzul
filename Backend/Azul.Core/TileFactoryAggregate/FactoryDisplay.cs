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
            _tiles.AddRange(tilesToAdd);
        }

        public IReadOnlyList<TileType> TakeTiles(TileType tileType)
        {
            if (!_tiles.Contains(tileType))
            {
                throw new InvalidOperationException("The requested tile type is not present in the display.");
            }

            var takenTiles = _tiles.Where(t => t == tileType).ToList();
            var toMoveToCenter = _tiles.Where(t => t != tileType).ToList();

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
