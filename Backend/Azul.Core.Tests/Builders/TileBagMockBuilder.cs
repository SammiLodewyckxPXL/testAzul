using Azul.Core.TileFactoryAggregate;
using Azul.Core.TileFactoryAggregate.Contracts;
using Moq;

namespace Azul.Core.Tests.Builders;

public class TileBagMockBuilder : MockBuilder<ITileBag>
{
    private readonly List<TileType> _allTiles;

    public TileBagMockBuilder()
    {
        _allTiles = new List<TileType>();
        AddTiles(20, TileType.BlackBlue);
        AddTiles(20, TileType.PlainBlue);
        AddTiles(20, TileType.PlainRed);
        AddTiles(20, TileType.WhiteTurquoise);
        AddTiles(20, TileType.YellowRed);

        Mock.SetupGet(b => b.Tiles).Returns(() => _allTiles);

        bool tryTakeResult = true;
        Mock.Setup(b => b.TryTakeTiles(It.IsAny<int>(), out It.Ref<IReadOnlyList<TileType>>.IsAny))
            .Callback((int amount, out IReadOnlyList<TileType> tiles) =>
            {
                if (_allTiles.Count < amount)
                {
                    tiles = new List<TileType>(_allTiles);
                    _allTiles.Clear();
                    tryTakeResult = false;
                }
                else
                {
                    tryTakeResult = true;
                    var takenTiles = new List<TileType>();
                    for (var i = 0; i < amount; i++)
                    {
                        int randomIndex = Random.Shared.Next(0, _allTiles.Count);
                        TileType takenTile = _allTiles[randomIndex];
                        _allTiles.RemoveAt(randomIndex);

                        takenTiles.Add(takenTile);
                    }
                    tiles = takenTiles;
                }
            })
            .Returns(() => tryTakeResult);
        Mock.Setup(b => b.AddTiles(It.IsAny<int>(), It.IsAny<TileType>())).Callback((int amount, TileType type) =>
        {
            for (var i = 0; i < amount; i++)
            {
                _allTiles.Add(type);
            }
        });
        Mock.Setup(b => b.AddTiles(It.IsAny<IReadOnlyList<TileType>>())).Callback((IReadOnlyList<TileType> tilesToAdd) =>
        {
            _allTiles.AddRange(tilesToAdd);
        });

    }

    public TileBagMockBuilder WithTiles(params TileType[] tiles)
    {
        _allTiles.Clear();
        _allTiles.AddRange(tiles);
        return this;
    }

    private void AddTiles(int amount, TileType type)
    {
        for (int i = 0; i < amount; i++)
        {
            _allTiles.Add(type);
        }
    }
}