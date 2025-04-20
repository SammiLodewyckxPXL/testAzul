using Azul.Core.BoardAggregate;
using Azul.Core.BoardAggregate.Contracts;
using Azul.Core.Tests.Extensions;
using Azul.Core.TileFactoryAggregate.Contracts;
using Guts.Client.Core;

namespace Azul.Core.Tests;

[ProjectComponentTestFixture("1TINProject", "Azul", "PatternLine",
    @"Azul.Core\BoardAggregate\PatternLine.cs;")]
public class PatternLineTests
{
    [MonitoredTest]
    public void Class_ShouldBeInternal_SoThatItCanOnlyBeUsedInTheCoreProject()
    {
        Assert.That(typeof(PatternLine).IsNotPublic, Is.True, "use 'internal class' instead of 'public class'");
    }

    [MonitoredTest]
    public void Class_ShouldImplement_IPatternLine()
    {
        Assert.That(typeof(PatternLine).IsAssignableTo(typeof(IPatternLine)), Is.True);
    }

    [MonitoredTest]
    public void IPatternLine_Interface_ShouldHaveCorrectMembers()
    {
        var type = typeof(IPatternLine);
        type.AssertInterfaceProperty(nameof(IPatternLine.Length), shouldHaveGetter: true, shouldHaveSetter: false);
        type.AssertInterfaceProperty(nameof(IPatternLine.TileType), shouldHaveGetter: true, shouldHaveSetter: false);
        type.AssertInterfaceProperty(nameof(IPatternLine.NumberOfTiles), shouldHaveGetter: true, shouldHaveSetter: false);
        type.AssertInterfaceProperty(nameof(IPatternLine.IsComplete), shouldHaveGetter: true, shouldHaveSetter: false);

        type.AssertInterfaceMethod(nameof(IPatternLine.Clear), returnType: typeof(void));
        type.AssertInterfaceMethod(nameof(IPatternLine.TryAddTiles), returnType: typeof(void), typeof(TileType), typeof(int), typeof(int));
    }

    [MonitoredTest]
    public void TryAddTiles_PatternLineAlreadyComplete_ShouldThrowInvalidOperationException()
    {
        Class_ShouldImplement_IPatternLine();

        //Arrange
        IPatternLine patternLine = new PatternLine(3) as IPatternLine;
        TileType type = Random.Shared.NextTileType();
        patternLine!.TryAddTiles(type, 3, out int remainingNumberOfTiles);
        string arrangeError = "Error when filling pattern line before executing the test";
        Assert.That(remainingNumberOfTiles, Is.Zero, arrangeError);
        Assert.That(patternLine.IsComplete, Is.True, arrangeError);
        Assert.That(patternLine.TileType, Is.EqualTo(type), arrangeError);
        Assert.That(patternLine.NumberOfTiles, Is.EqualTo(3), arrangeError);

        //Act + Assert
        Assert.That(() => patternLine.TryAddTiles(type, 1, out remainingNumberOfTiles),
            Throws.InvalidOperationException.With.Message.ContainsOne("complete", "compleet"));
    }

    [MonitoredTest]
    public void TryAddTiles_PatternLineAlreadyContainsTileOfOtherType_ShouldThrowInvalidOperationException()
    {
        //Arrange
        var patternLine = new PatternLine(3) as IPatternLine;
        TileType existingType = TileType.BlackBlue;
        patternLine!.TryAddTiles(existingType, 1, out int remainingNumberOfTiles);
        string arrangeError = "Error when filling pattern line before executing the test";
        Assert.That(remainingNumberOfTiles, Is.EqualTo(0), arrangeError);
        Assert.That(patternLine.IsComplete, Is.False, arrangeError);
        Assert.That(patternLine.TileType, Is.EqualTo(existingType), arrangeError);
        Assert.That(patternLine.NumberOfTiles, Is.EqualTo(1), arrangeError);
        TileType type = Random.Shared.NextItem(Enum.GetValues<TileType>().Where(t => t != TileType.StartingTile && t != TileType.BlackBlue));
        
        //Act + Assert
        Assert.That(() => patternLine.TryAddTiles(type, 1, out remainingNumberOfTiles),
            Throws.InvalidOperationException.With.Message.ContainsOne("type"));
    }
}