using Azul.Core.TileFactoryAggregate.Contracts;
using System;

namespace Azul.Core.BoardAggregate
{
    public class TileSpot
    {
        public TileType? Type { get; private set; }

        public bool HasTile { get; private set; }

        private readonly bool _isTypeFixed;

        public TileSpot(TileType? type = null, bool isTypeFixed = false)
        {
            Type = type;
            HasTile = false;
            _isTypeFixed = isTypeFixed;
        }

        public void PlaceTile(TileType type, bool force = false)
        {
            // If not forcing, validate that the correct tile type is placed
            if (!force)
            {
                if (Type.HasValue && Type.Value != type)
                {
                    throw new InvalidOperationException("Cannot place a tile of a different type on this spot.");
                }

                if (HasTile)
                {
                    throw new InvalidOperationException("A tile is already placed on this spot.");
                }
            }

            // If the type is not fixed, allow modification
            if (!_isTypeFixed)
            {
                Type = type;  // Update the type if it's not fixed
            }
            else
            {
                // If the type is fixed, do not change the existing type
                if (Type.HasValue && Type.Value != type)
                {
                    throw new InvalidOperationException("Cannot place a tile of a different type on this spot, as the type is fixed.");
                }
            }

            // Mark the spot as having a tile
            HasTile = true;
        }


        public void Clear()
        {
            if (!_isTypeFixed)
            {
                Type = null;
            }

            HasTile = false;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not TileSpot other) return false;
            return Type == other.Type;
        }

        public override int GetHashCode()
        {
            return Type?.GetHashCode() ?? 0;
        }
    }
}
