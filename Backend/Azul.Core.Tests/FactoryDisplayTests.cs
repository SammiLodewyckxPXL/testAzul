using Azul.Core.Tests.Extensions;
using Azul.Core.TileFactoryAggregate;
using Azul.Core.TileFactoryAggregate.Contracts;
using Guts.Client.Core;

namespace Azul.Core.Tests;

[ProjectComponentTestFixture("1TINProject", "Azul", "FactoryDisplay",
    @"Azul.Core\TileFactoryAggregate\FactoryDisplay.cs;")]
public class FactoryDisplayTests
{
    [MonitoredTest]
    public void FactoryDisplay_Class_ShouldBeInternal_SoThatItCanOnlyBeUsedInTheCoreProject()
    {
        Assert.That(typeof(FactoryDisplay).IsNotPublic, Is.True, "use 'internal class' instead of 'public class'");
    }

    [MonitoredTest]
    public void FactoryDisplay_Class_ShouldImplement_IFactoryDisplay()
    {
        Assert.That(typeof(FactoryDisplay).IsAssignableTo(typeof(IFactoryDisplay)), Is.True);
    }

    [MonitoredTest]
    public void IFactoryDisplay_Interface_ShouldHaveCorrectMembers()
    {
        var type = typeof(IFactoryDisplay);

        type.AssertInterfaceProperty(nameof(IFactoryDisplay.Id), shouldHaveGetter: true, shouldHaveSetter: false);
        type.AssertInterfaceProperty(nameof(IFactoryDisplay.Tiles), shouldHaveGetter: true, shouldHaveSetter: false);
        type.AssertInterfaceProperty(nameof(IFactoryDisplay.IsEmpty), shouldHaveGetter: true, shouldHaveSetter: false);

        type.AssertInterfaceMethod(nameof(IFactoryDisplay.AddTiles), typeof(void), typeof(IReadOnlyList<TileType>));
        type.AssertInterfaceMethod(nameof(IFactoryDisplay.TakeTiles), typeof(IReadOnlyList<TileType>), typeof(TileType));
    }
}