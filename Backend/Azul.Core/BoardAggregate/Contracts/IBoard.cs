using Azul.Core.TileFactoryAggregate.Contracts;

namespace Azul.Core.BoardAggregate.Contracts;

public interface IBoard
{
    public IPatternLine[] PatternLines { get; }
    public TileSpot[,] Wall { get; }
    public TileSpot[] FloorLine { get; }
    int Score { get; }

    /// <summary>
    /// Checks if the player has completed a horizontal line on the wall.
    /// </summary>
    bool HasCompletedHorizontalLine { get; }

    /// <summary>
    /// Adds tiles to the pattern line.
    /// Excess tiles are added to the floor line.
    /// If the floor line is full, the excess tiles are added to the used tiles of the <paramref name="tileFactory"/>.
    /// </summary>
    /// <param name="tilesToAdd">The tiles to add to the pattern line</param>
    /// <param name="patternLineIndex">The index of the pattern line (0 is the top line, 4 is the bottom line)</param>
    /// <param name="tileFactory">
    /// Tiles that cannot be placed on the pattern line and also not on the floor line, should be added to the used tiles of the factory.
    /// </param>
    void AddTilesToPatternLine(IReadOnlyList<TileType> tilesToAdd, int patternLineIndex, ITileFactory tileFactory);

    /// <summary>
    /// Adds tiles to the floor line.
    /// If the floor line is full, the excess tiles are added to the used tiles of the <paramref name="tileFactory"/>.
    /// </summary>
    /// <param name="tilesToAdd">The tiles to add to the floor line</param>
    /// <param name="tileFactory">
    /// Tiles that cannot be placed on the floor line, should be added to the used tiles of the factory.
    /// </param>
    void AddTilesToFloorLine(IReadOnlyList<TileType> tilesToAdd, ITileFactory tileFactory);

    /// <summary>
    /// Moved tiles from completed pattern lines to the wall and calculates the score.
    /// The floor line is also cleared and the point loss is calculated.
    /// Excess tiles are added to the used tiles of the <paramref name="tileFactory"/>.
    /// </summary>
    void DoWallTiling(ITileFactory tileFactory);

    /// <summary>
    /// Adds bonus points to the <see cref="Score"/>.
    /// Points are given
    /// - for completed horizontal lines (2 points)
    /// - completed vertical lines (7 points)
    /// - completed colors (10 points).
    /// </summary>
    void CalculateFinalBonusScores();
}