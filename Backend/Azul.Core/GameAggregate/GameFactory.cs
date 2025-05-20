using Azul.Core.GameAggregate.Contracts;
using Azul.Core.PlayerAggregate.Contracts;
using Azul.Core.TableAggregate.Contracts;
using Azul.Core.TileFactoryAggregate;
using Azul.Core.TileFactoryAggregate.Contracts;
using System;
using System.Linq;

namespace Azul.Core.GameAggregate;

internal class GameFactory : IGameFactory
{
    public IGame CreateNewForTable(ITable table)
    {
        var players = table.SeatedPlayers.ToArray();

        var bag = new TileBag();
        foreach (TileType tileType in Enum.GetValues<TileType>())
        {
            if (tileType != TileType.StartingTile)
            {
                bag.AddTiles(20, tileType);
            }
        }

        int numberOfDisplays = table.Preferences.NumberOfFactoryDisplays;
        var tileFactory = new TileFactory(numberOfDisplays, bag);
        Console.WriteLine(bag.Tiles.Count);
        //tileFactory.FillDisplays();
        Console.WriteLine(bag.Tiles.Count);
        var game = new Game(Guid.NewGuid(), tileFactory, players);
        return game;
    }
}
