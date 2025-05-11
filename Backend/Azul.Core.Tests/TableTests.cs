using System.Drawing;
using Azul.Core.PlayerAggregate.Contracts;
using Azul.Core.TableAggregate;
using Azul.Core.TableAggregate.Contracts;
using Azul.Core.Tests.Builders;
using Azul.Core.Tests.Extensions;
using Azul.Core.UserAggregate;
using Guts.Client.Core;

namespace Azul.Core.Tests;

[ProjectComponentTestFixture("1TINProject", "Azul", "Table",
    @"Azul.Core\TableAggregate\Table.cs;")]
public class TableTests
{
    private Guid _id;
    private TablePreferences _defaultPreferences = null!;
    private ITable? _table;

    [SetUp]
    public void SetUp()
    {
        _id = Guid.NewGuid();
        _defaultPreferences = new TablePreferences();
        _table = new Table(_id, _defaultPreferences) as ITable;
    }

    [MonitoredTest]
    public void Class_ShouldBeInternal_SoThatItCanOnlyBeUsedInTheCoreProject()
    {
        Assert.That(typeof(Table).IsNotPublic, Is.True, "use 'internal class' instead of 'public class'");
    }

    [MonitoredTest]
    public void Class_ShouldImplement_ITable()
    {
        Assert.That(typeof(Table).IsAssignableTo(typeof(ITable)), Is.True);
    }

    [MonitoredTest]
    public void ITable_Interface_ShouldHaveCorrectMembers()
    {
        var type = typeof(ITable);

        type.AssertInterfaceProperty(nameof(ITable.Id), shouldHaveGetter: true, shouldHaveSetter: false);
        type.AssertInterfaceProperty(nameof(ITable.Preferences), shouldHaveGetter: true, shouldHaveSetter: false);
        type.AssertInterfaceProperty(nameof(ITable.SeatedPlayers), shouldHaveGetter: true, shouldHaveSetter: false);
        type.AssertInterfaceProperty(nameof(ITable.HasAvailableSeat), shouldHaveGetter: true, shouldHaveSetter: false);
        type.AssertInterfaceProperty(nameof(ITable.GameId), shouldHaveGetter: true, shouldHaveSetter: true);

        type.AssertInterfaceMethod(nameof(ITable.Join), typeof(void), typeof(User));
        type.AssertInterfaceMethod(nameof(ITable.Leave), typeof(void), typeof(Guid));
        type.AssertInterfaceMethod(nameof(ITable.FillWithArtificialPlayers), typeof(void), typeof(IGamePlayStrategy));
    }

    [MonitoredTest]
    public void Constructor_ShouldInitializeProperties()
    {
        Assert.That(_table, Is.Not.Null, "Table should implement ITable");
        Assert.That(_table!.Id, Is.EqualTo(_id), "Id is not set properly");
        Assert.That(_table.Preferences, Is.EqualTo(_defaultPreferences), "Preferences are not set properly");
        Assert.That(_table.HasAvailableSeat, Is.True, "A newly created table should have seats available");
        Assert.That(_table.SeatedPlayers, Is.Not.Null, "SeatedPlayers should be initialized");
        Assert.That(_table.SeatedPlayers.Count, Is.EqualTo(0), "There should be no seated players");
        Assert.That(_table.GameId, Is.EqualTo(Guid.Empty), "GameId should be Guid.Empty");
    }

    [MonitoredTest]
    public void Join_FirstUserJoins_ShouldAddUserToSeatedPlayers()
    {
        Assert.That(_table, Is.Not.Null, "Table should implement ITable");

        // Arrange
        User user = new UserBuilder().Build();

        // Act
        _table!.Join(user);

        // Assert
        Assert.That(_table.SeatedPlayers.Count, Is.EqualTo(1), "There should be 1 seated player");
        IPlayer seatedPlayer = _table.SeatedPlayers[0];
        Assert.That(seatedPlayer.Id, Is.EqualTo(user.Id), "The seated player has an incorrect id");
        Assert.That(seatedPlayer.Name, Is.EqualTo(user.UserName), "The seated player has an incorrect name");
        Assert.That(seatedPlayer.LastVisitToPortugal, Is.EqualTo(user.LastVisitToPortugal),
            "The seated player has an incorrect last visit to Portugal date");

    }

    [MonitoredTest]
    public void Join_SecondUserJoins_ShouldAddUserToSeatedPlayers_ShouldHaveNoAvailableSeatsLeft()
    {
        Assert.That(_table, Is.Not.Null, "Table should implement ITable");

        // Arrange
        User user1 = new UserBuilder().Build();
        User user2 = new UserBuilder().Build();
        _table!.Join(user1);

        // Act
        _table.Join(user2);

        // Assert
        Assert.That(_table.SeatedPlayers.Count, Is.EqualTo(2), "There should be 2 seated players");
        IPlayer? secondPlayer = _table.SeatedPlayers.FirstOrDefault(p => p.Id == user2.Id);
        Assert.That(secondPlayer, Is.Not.Null, "The second player (with same id as the second user) should be seated");
        Assert.That(secondPlayer!.Name, Is.EqualTo(user2.UserName), "The second seated player has an incorrect name");
        Assert.That(secondPlayer.LastVisitToPortugal, Is.EqualTo(user2.LastVisitToPortugal),
            "The second player  has an incorrect last visit to Portugal date");
        Assert.That(_table.HasAvailableSeat, Is.False, "The table should be full");
    }

    [MonitoredTest]
    public void Join_UserJoinsTwice_ShouldThrowInvalidOperationException()
    {
        Assert.That(_table, Is.Not.Null, "Table should implement ITable");

        // Arrange
        User user = new UserBuilder().Build();
        _table!.Join(user);

        // Act + Assert
        Assert.That(() => _table.Join(user), Throws.InvalidOperationException);
        Assert.That(() => _table.Join(user),
           Throws.InvalidOperationException.With.Message.ContainsOne("already seated", "zit al", "zit reeds"));
    }

    [MonitoredTest]
    public void Join_TableIsFull_ShouldThrowInvalidOperationException()
    {
        Assert.That(_table, Is.Not.Null, "Table should implement ITable");

        // Arrange
        var user1 = new UserBuilder().Build();
        var user2 = new UserBuilder().Build();
        var user3 = new UserBuilder().Build();
        _table!.Join(user1);
        _table.Join(user2);

        // Act + Assert
        Assert.That(() => _table.Join(user3), Throws.InvalidOperationException);
        Assert.That(() => _table.Join(user3),
            Throws.InvalidOperationException.With.Message.ContainsOne("full", "vol"));
    }

    [MonitoredTest]
    public void Leave_ShouldRemoveUserFromSeatedPlayers()
    {
        Assert.That(_table, Is.Not.Null, "Table should implement ITable");

        // Arrange
        var user1 = new UserBuilder().Build();
        var user2 = new UserBuilder().Build();
        _table!.Join(user1);
        _table.Join(user2);

        // Act
        _table.Leave(user1.Id);

        // Assert
        Assert.That(_table.SeatedPlayers.Count, Is.EqualTo(1), "There should be 1 seated player left");
        Assert.That(_table.SeatedPlayers[0].Id, Is.EqualTo(user2.Id), "The second user should still be seated");
    }

    [MonitoredTest]
    public void Leave_UserThatIsNotSeatedTriesToLeave_ShouldThrowInvalidOperationException()
    {
        Assert.That(_table, Is.Not.Null, "Table should implement ITable");

        // Arrange
        var user1 = new UserBuilder().Build();
        var user2 = new UserBuilder().Build();
        _table!.Join(user1);

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() => _table.Leave(user2.Id));
    }
}