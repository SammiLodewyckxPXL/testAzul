using Azul.Core.Tests.Extensions;
using Azul.Core.TileFactoryAggregate;
using Azul.Core.TileFactoryAggregate.Contracts;
using Guts.Client.Core;

namespace Azul.Core.Tests;

[ProjectComponentTestFixture("1TINProject", "Azul", "TableCenter",
    @"Azul.Core\TileFactoryAggregate\TableCenter.cs;")]
public class TableCenterTests
{
    [MonitoredTest]
    public void TableCenter_Class_ShouldBeInternal_SoThatItCanOnlyBeUsedInTheCoreProject()
    {
        Assert.That(typeof(TableCenter).IsNotPublic, Is.True, "use 'internal class' instead of 'public class'");
    }

    [MonitoredTest]
    public void TableCenter_Class_ShouldImplement_ITableCenter()
    {
        Assert.That(typeof(TableCenter).IsAssignableTo(typeof(ITableCenter)), Is.True);
    }

    [MonitoredTest]
    public void ITableCenter_Interface_ShouldImplement_IFactoryDisplay()
    {
        Assert.That(typeof(ITableCenter).IsAssignableTo(typeof(IFactoryDisplay)), Is.True);
    }

    [MonitoredTest]
    public void ITableCenter_Interface_ShouldHaveCorrectMembers()
    {
        var type = typeof(ITableCenter);
        type.AssertInterfaceMethod(nameof(ITableCenter.AddStartingTile), typeof(void));
    }
}