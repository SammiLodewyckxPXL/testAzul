using Azul.Core.TileFactoryAggregate.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Azul.Core.TileFactoryAggregate;

internal class TileFactory : ITileFactory
{
    public ITileBag Bag { get; }

    public IReadOnlyList<IFactoryDisplay> Displays { get; }

    public ITableCenter TableCenter { get; }

    public IReadOnlyList<TileType> UsedTiles => _usedTiles.AsReadOnly();

    public bool IsEmpty => Displays.All(d => d.IsEmpty) && TableCenter.IsEmpty;

    private readonly List<TileType> _usedTiles = new();

    private readonly Dictionary<Guid, IFactoryDisplay> _displayLookup = new();

    public TileFactory(int numberOfDisplays, ITileBag bag)
    {
        Bag = bag;

        TableCenter = new TableCenter();

        var displays = new List<IFactoryDisplay>();
        for (int i = 0; i < numberOfDisplays; i++)
        {
            var display = new FactoryDisplay(TableCenter);
            displays.Add(display);
            _displayLookup.Add(display.Id, display);
        }

        Displays = displays.AsReadOnly();

        _displayLookup.Add(TableCenter.Id, TableCenter);
    }

    public void FillDisplays()
    {
        foreach (var display in Displays)
        {
            display.AddTiles(Array.Empty<TileType>());

            if (!Bag.TryTakeTiles(4, out var tilesTaken) || tilesTaken.Count < 4)
            {
                int stillNeeded = 4 - tilesTaken.Count;

                if (_usedTiles.Any())
                {
                    Bag.AddTiles(_usedTiles);
                    _usedTiles.Clear();

                    Bag.TryTakeTiles(stillNeeded, out var additionalTiles);
                    tilesTaken = tilesTaken.Concat(additionalTiles).ToList();
                }
            }

            display.AddTiles(tilesTaken);
        }

        TableCenter.AddTiles(Array.Empty<TileType>());
    }


    public IReadOnlyList<TileType> TakeTiles(Guid displayId, TileType tileType)
    {
        if (!_displayLookup.TryGetValue(displayId, out var display))
        {
            throw new InvalidOperationException("The display with the given ID does not exist.");
        }

        if (!display.Tiles.Contains(tileType))
        {
            throw new InvalidOperationException("The selected tile type is not present in the display.");
        }

        var takenTiles = display.Tiles.Where(t => t == tileType).ToList();
        var remainingTiles = display.Tiles.Where(t => t != tileType).ToList();

        display.AddTiles(Array.Empty<TileType>());

        if (display is ITableCenter)
        {
            var withStartTile = new List<TileType>(takenTiles);
            if (remainingTiles.Contains(TileType.StartingTile))
            {
                withStartTile.Insert(0, TileType.StartingTile);
                remainingTiles.Remove(TileType.StartingTile);
            }

            TableCenter.AddTiles(remainingTiles);
            return withStartTile;
        }
        else
        {
            TableCenter.AddTiles(remainingTiles);
            return takenTiles;
        }
    }

    public void AddToUsedTiles(TileType tile)
    {
        _usedTiles.Add(tile);
    }
}
