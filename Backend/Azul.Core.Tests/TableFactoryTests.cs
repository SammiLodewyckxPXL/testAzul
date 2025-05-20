using Azul.Core.TableAggregate;
using Azul.Core.TableAggregate.Contracts;
using Azul.Core.Tests.Builders;
using Azul.Core.Tests.Extensions;
using Azul.Core.UserAggregate;
using Guts.Client.Core;

namespace Azul.Core.Tests;

[ProjectComponentTestFixture("1TINProject", "Azul", "TableFactory",
    @"Azul.Core\TableAggregate\TableFactory.cs;")]
public class TableFactoryTests
{
    private ITableFactory? _tableFactory;

    [SetUp]
    public void SetUp()
    {
        _tableFactory = new TableFactory() as ITableFactory;
    }

    [MonitoredTest]
    public void Class_ShouldBeInternal_SoThatItCanOnlyBeUsedInTheCoreProject()
    {
        Assert.That(typeof(TableFactory).IsNotPublic, Is.True, "use 'internal class' instead of 'public class'");
    }

    [MonitoredTest]
    public void Class_ShouldImplement_ITableFactory()
    {
        Assert.That(typeof(TableFactory).IsAssignableTo(typeof(ITableFactory)), Is.True,
            "TableFactory should implement ITableFactory");
    }

    [MonitoredTest]
    public void ITableFactory_Interface_ShouldHaveCorrectMembers()
    {
        var type = typeof(ITableFactory);
        type.AssertInterfaceMethod(nameof(ITableFactory.CreateNewForUser), typeof(ITable), typeof(User), typeof(ITablePreferences));
    }

    [MonitoredTest]
    public void CreateNewForUser_ShouldCreateTableAndJoinUser()
    {
        Class_ShouldImplement_ITableFactory();

        // Arrange
        var user = new UserBuilder().Build();
        var preferences = new TablePreferences();

        // Act
        var table = _tableFactory!.CreateNewForUser(user, preferences);

        // Assert
        Assert.That(table.Id, Is.Not.EqualTo(Guid.Empty), "A non-empty Guid must be used for the id");
        Assert.That(table.Preferences, Is.EqualTo(preferences), "The provided preferences must be assigned to the table");
        Assert.That(table.SeatedPlayers.Count, Is.EqualTo(1), "A player should be seated for the user");
        Assert.That(table.SeatedPlayers[0].Id, Is.EqualTo(user.Id), "A player should be seated for the user");
    }
}