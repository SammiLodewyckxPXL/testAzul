using Azul.Core.GameAggregate.Contracts;

namespace Azul.Core.GameAggregate;

/// <inheritdoc cref="IPlaceTilesMove"/>
internal class PlaceTilesMove : IPlaceTilesMove
{
    public bool PlaceInFloorLine { get; }
    public int PatternLineIndex { get; }

    private PlaceTilesMove(bool placeInFloorLine, int patternLineIndex)
    {
        PlaceInFloorLine = placeInFloorLine;
        PatternLineIndex = patternLineIndex;
    }

    public static PlaceTilesMove CreateMoveOnPatternLine(int patternLineIndex)
    {
        return new PlaceTilesMove(false, patternLineIndex);
    }

    public static PlaceTilesMove CreateMoveOnFloorLine()
    {
        return new PlaceTilesMove(true, -1);
    }
}