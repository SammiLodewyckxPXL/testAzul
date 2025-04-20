namespace Azul.Core.TileFactoryAggregate.Contracts;

/// <summary>
/// Represents a bag of tiles that can used to randomly take a certain amount tiles.
/// The bag can also be filled with tiles.
/// </summary>
public interface ITileBag
{
    /// <summary>
    /// The tiles that are currently in the bag.
    /// </summary>
    public IReadOnlyList<TileType> Tiles { get; }

    /// <summary>
    /// Adds a certain amount of tiles of a certain type to the bag.
    /// </summary>
    public void AddTiles(int amount, TileType tileType);

    /// <summary>
    /// Adds a list of tiles to the bag (e.g. the used tiles in the <see cref="ITileFactory"/>).
    /// </summary>
    public void AddTiles(IReadOnlyList<TileType> tilesToAdd);

    /// <summary>
    /// Tries to take a certain amount of tiles from the bag.
    /// </summary>
    /// <param name="amount">Amount of tiles to take</param>
    /// <param name="tiles">
    /// The tiles that were taken.
    /// It could be less than the <paramref name="amount"/> that was asked if the bag does not contain enough tiles.
    /// Will be an empty list if the bag is empty.
    /// </param>
    /// <returns>
    /// True if the bag contained enough tiles to take the requested <paramref name="amount"/>.
    /// False if the bag did not contain enough tiles to take the requested <paramref name="amount"/>.
    /// </returns>
    public bool TryTakeTiles(int amount, out IReadOnlyList<TileType> tiles);
}