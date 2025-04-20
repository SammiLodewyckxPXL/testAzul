using Azul.Core.PlayerAggregate;
using Azul.Core.PlayerAggregate.Contracts;
using Azul.Core.Tests.Extensions;
using Guts.Client.Core;

namespace Azul.Core.Tests;

[ProjectComponentTestFixture("1TINProject", "Azul", "Player",
    @"Azul.Core\PlayerAggregate\PlayerBase.cs;
Azul.Core\PlayerAggregate\HumanPlayer.cs;")]
public class PlayerTests
{
    [MonitoredTest]
    public void HumanPlayer_Class_ShouldBeInternal_SoThatItCanOnlyBeUsedInTheCoreProject()
    {
        Assert.That(typeof(HumanPlayer).IsNotPublic, Is.True, "use 'internal class' instead of 'public class'");
    }

    [MonitoredTest]
    public void HumanPlayer_Class_ShouldInheritFromPlayerBase()
    {
        Assert.That(typeof(HumanPlayer).IsAssignableTo(typeof(PlayerBase)), Is.True);
    }

    [MonitoredTest]
    public void PlayerBase_Class_ShouldBeInternal_SoThatItCanOnlyBeUsedInTheCoreProject()
    {
        Assert.That(typeof(PlayerBase).IsNotPublic, Is.True, "use 'internal class' instead of 'public class'");
    }

    [MonitoredTest]
    public void PlayerBase_Class_ShouldImplement_IPlayer()
    {
        Assert.That(typeof(PlayerBase).IsAssignableTo(typeof(IPlayer)), Is.True);
    }

    [MonitoredTest]
    public void IPlayer_Interface_ShouldHaveCorrectMembers()
    {
        var type = typeof(IPlayer);
        type.AssertInterfaceProperty(nameof(IPlayer.Id), shouldHaveGetter: true, shouldHaveSetter: false);
        type.AssertInterfaceProperty(nameof(IPlayer.Name), shouldHaveGetter: true, shouldHaveSetter: false);
        type.AssertInterfaceProperty(nameof(IPlayer.LastVisitToPortugal), shouldHaveGetter: true, shouldHaveSetter: false);
        type.AssertInterfaceProperty(nameof(IPlayer.Board), shouldHaveGetter: true, shouldHaveSetter: false);
        type.AssertInterfaceProperty(nameof(IPlayer.HasStartingTile), shouldHaveGetter: true, shouldHaveSetter: true);
        type.AssertInterfaceProperty(nameof(IPlayer.TilesToPlace), shouldHaveGetter: true, shouldHaveSetter: false);
    }

    [MonitoredTest]
    public void PlayerBase_Constructor_ShouldInitializeProperties()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        string name = "John Doe";
        DateOnly? lastVisitToPortugal = new DateOnly(2021, 1, 1);
        
        //Act
        IPlayer? testPlayer = new TestPlayer(userId, name, lastVisitToPortugal) as IPlayer;

        //Assert
        Assert.That(testPlayer, Is.Not.Null, "PlayerBase should implement IPlayer");
        Assert.That(testPlayer!.Id, Is.EqualTo(userId), "Id is not set properly");
        Assert.That(testPlayer.Name, Is.EqualTo(name), "Name is not set properly");
        Assert.That(testPlayer.LastVisitToPortugal, Is.EqualTo(lastVisitToPortugal), "LastVisitToPortugal is not set properly");
        Assert.That(testPlayer.Board, Is.Not.Null, "The Board is not set properly");
        Assert.That(testPlayer.HasStartingTile, Is.False, "HasStartingTile is not set properly");
        Assert.That(testPlayer.TilesToPlace, Is.Not.Null, "TilesToPlace is not set properly");
        Assert.That(testPlayer.TilesToPlace.Count, Is.EqualTo(0), "TilesToPlace should be empty");
    }

    private class TestPlayer(Guid id, string name, DateOnly? lastVisitToPortugal)
        : PlayerBase(id, name, lastVisitToPortugal);
}