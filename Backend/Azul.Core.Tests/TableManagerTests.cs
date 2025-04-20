using Azul.Core.GameAggregate.Contracts;
using Azul.Core.PlayerAggregate.Contracts;
using Azul.Core.TableAggregate;
using Azul.Core.TableAggregate.Contracts;
using Azul.Core.Tests.Builders;
using Azul.Core.Tests.Extensions;
using Azul.Core.UserAggregate;
using Azul.Core.Util;
using Guts.Client.Core;
using Moq;

namespace Azul.Core.Tests
{
    [ProjectComponentTestFixture("1TINProject", "Azul", "TableManager",
        @"Azul.Core\TableAggregate\TableManager.cs;")]
    public class TableManagerTests
    {
        private Mock<ITableRepository> _tableRepositoryMock = null!;
        private Mock<ITableFactory> _tableFactoryMock = null!;
        private Mock<IGameRepository> _gameRepositoryMock = null!;
        private Mock<IGameFactory> _gameFactoryMock = null!;
        private Mock<IGamePlayStrategy> _gamePlayStrategyMock = null!;
        private TableManager _tableManager = null!;

        [SetUp]
        public void Setup()
        {
            _tableRepositoryMock = new Mock<ITableRepository>();
            _tableFactoryMock = new Mock<ITableFactory>();
            _gameRepositoryMock = new Mock<IGameRepository>();
            _gameFactoryMock = new Mock<IGameFactory>();
            _gamePlayStrategyMock = new Mock<IGamePlayStrategy>();

            _tableManager = new TableManager(
                _tableRepositoryMock.Object,
                _tableFactoryMock.Object,
                _gameRepositoryMock.Object,
                _gameFactoryMock.Object,
                _gamePlayStrategyMock.Object);
        }

        [MonitoredTest]
        public void Class_ShouldBeInternal_SoThatItCanOnlyBeUsedInTheCoreProject()
        {
            Assert.That(typeof(TableManager).IsNotPublic, Is.True, "use 'internal class' instead of 'public class'");
        }

        [MonitoredTest]
        public void Class_ShouldImplement_ITableManager()
        {
            Assert.That(typeof(TableManager).IsAssignableTo(typeof(ITableManager)), Is.True);
        }

        [MonitoredTest]
        public void ITableManager_Interface_ShouldHaveCorrectMembers()
        {
            var type = typeof(ITableManager);

            type.AssertInterfaceMethod(nameof(ITableManager.JoinOrCreateTable), typeof(ITable), typeof(User), typeof(ITablePreferences));
            type.AssertInterfaceMethod(nameof(ITableManager.LeaveTable), typeof(void), typeof(Guid), typeof(User));
            type.AssertInterfaceMethod(nameof(ITableManager.FillWithArtificialPlayers), typeof(void), typeof(Guid), typeof(User));
            type.AssertInterfaceMethod(nameof(ITableManager.StartGameForTable), typeof(IGame), typeof(Guid));
        }

        [MonitoredTest]
        public void JoinOrCreateTable_NoTableWithAvailableSeatsInRepository_ShouldCreateANewTableAndAddItToTheRepository()
        {
            // Arrange
            User user = new UserBuilder().Build();
            TablePreferences preferences = new TablePreferencesBuilder().Build();
            ITable table = new TableMockBuilder().Object;
            _tableFactoryMock.Setup(f => f.CreateNewForUser(It.IsAny<User>(), It.IsAny<TablePreferences>())).Returns(table);

            _tableRepositoryMock.Setup(r => r.FindTablesWithAvailableSeats(It.IsAny<ITablePreferences>())).Returns(new List<ITable>());

            // Act
            var result = _tableManager.JoinOrCreateTable(user, preferences);

            // Assert
            _tableRepositoryMock.Verify(r => r.FindTablesWithAvailableSeats(preferences), Times.Once,
                "The repository should be used to retrieve available tables");
            _tableFactoryMock.Verify(f => f.CreateNewForUser(user, preferences), Times.Once,
                "The factory should be used (correctly) to create a table");
            _tableRepositoryMock.Verify(r => r.Add(table), Times.Once,
                "The created table should be added to the table repository");
            Assert.That(result, Is.SameAs(table), "The created table should be returned");
        }

        [MonitoredTest]
        public void JoinOrCreateTable_TableWithOneAvailableSeatExistsInRepository_ShouldAddUserToTable()
        {
            // Arrange
            User user = new UserBuilder().Build();
            TablePreferences preferences = new TablePreferencesBuilder().Build();
            var tableMockBuilder = new TableMockBuilder();
            Mock<ITable> tableMock = tableMockBuilder.Mock;
            ITable table = tableMockBuilder.Object;

            _tableRepositoryMock.Setup(r => r.FindTablesWithAvailableSeats(It.IsAny<ITablePreferences>())).Returns([table]);

            // Act
            _tableManager.JoinOrCreateTable(user, preferences);

            // Assert
            _tableRepositoryMock.Verify(r => r.FindTablesWithAvailableSeats(preferences), Times.Once,
                "The repository should be used to retrieve available tables");
            tableMock.Verify(t => t.Join(user), Times.Once, "User is not joined to the table correctly");
        }

        [MonitoredTest]
        public void LeaveTable_FirstOfTwoPlayersLeaves_ShouldRemovePlayerAssociatedWithUserFromTable()
        {
            // Arrange
            User user1 = new UserBuilder().Build();
            User user2 = new UserBuilder().Build();
            var tableMockBuilder = new TableMockBuilder().WithSeatedUsers([user1, user2]);
            Mock<ITable> tableMock = tableMockBuilder.Mock;
            ITable table = tableMockBuilder.Object;

            _tableRepositoryMock.Setup(r => r.Get(It.Is<Guid>(id => id != table.Id))).Throws<DataNotFoundException>();
            _tableRepositoryMock.Setup(r => r.Get(table.Id)).Returns(table);

            // Act
            _tableManager.LeaveTable(table.Id, user2);

            // Assert
            _tableRepositoryMock.Verify(repository => repository.Get(table.Id), Times.Once, "Table is not retrieved correctly");
            tableMock.Verify(t => t.Leave(user2.Id), Times.Once, "The 'Leave' method of the table is not called correctly");
            _tableRepositoryMock.Verify(repository => repository.Remove(It.IsAny<Guid>()), Times.Never,
                "The table should not be removed from the repository when there is still a player present");
        }

        [MonitoredTest]
        public void LeaveTable_LastPlayersLeaves_ShouldRemoveTableFromRepository()
        {
            // Arrange
            User user = new UserBuilder().Build();
            var tableMockBuilder = new TableMockBuilder().WithSeatedUsers([user]);
            Mock<ITable> tableMock = tableMockBuilder.Mock;
            ITable table = tableMockBuilder.Object;

            _tableRepositoryMock.Setup(r => r.Get(It.Is<Guid>(id => id != table.Id))).Throws<DataNotFoundException>();
            _tableRepositoryMock.Setup(r => r.Get(table.Id)).Returns(table);

            // Act
            _tableManager.LeaveTable(table.Id, user);

            // Assert
            _tableRepositoryMock.Verify(repository => repository.Get(table.Id), Times.Once, "Table is not retrieved correctly");
            tableMock.Verify(t => t.Leave(user.Id), Times.Once, "The 'Leave' method of the table is not called correctly");
            _tableRepositoryMock.Verify(repository => repository.Remove(table.Id), Times.Once,
                "The table is not removed correctly from the repository");
        }

        [MonitoredTest]
        public void StartGameForTable_ShouldUseFactoryToCreateAGameAndAddItToTheRepository()
        {
            // Arrange
            User user1 = new UserBuilder().Build();
            User user2 = new UserBuilder().Build();
            var tableMockBuilder = new TableMockBuilder().WithSeatedUsers([user1, user2]);
            Mock<ITable> tableMock = tableMockBuilder.Mock;
            ITable table = tableMockBuilder.Object;
            
            _tableRepositoryMock.Setup(r => r.Get(It.Is<Guid>(id => id != table.Id))).Throws<DataNotFoundException>();
            _tableRepositoryMock.Setup(r => r.Get(table.Id)).Returns(table);

            IGame createdGame = new GameMockBuilder().Object;
            _gameFactoryMock.Setup(f => f.CreateNewForTable(table)).Returns(createdGame);

            // Act
            IGame returnedGame = _tableManager.StartGameForTable(table.Id);

            // Assert
            _tableRepositoryMock.Verify(repository => repository.Get(table.Id), Times.Once, "Table is not retrieved correctly");
            _gameFactoryMock.Verify(f => f.CreateNewForTable(table), Times.Once, "The 'CreateNewForTable' method of the game factory is not called correctly");
            tableMock.VerifySet(t => t.GameId = returnedGame.Id, Times.Once, "The GameId of the table is not set correctly");
            _gameRepositoryMock.Verify(r => r.Add(createdGame), Times.Once, "The created game is not added to the game repository");
            Assert.That(returnedGame, Is.SameAs(createdGame), "The created game is not returned correctly");
        }

        [MonitoredTest]
        public void StartGameForTable_TableIsNotFull_ShouldThrowInvalidOperationException()
        {
            // Arrange
            User user1 = new UserBuilder().Build();
            var tableMockBuilder = new TableMockBuilder().WithSeatedUsers([user1]);
            Mock<ITable> tableMock = tableMockBuilder.Mock;
            ITable table = tableMockBuilder.Object;

            _tableRepositoryMock.Setup(r => r.Get(It.Is<Guid>(id => id != table.Id))).Throws<DataNotFoundException>();
            _tableRepositoryMock.Setup(r => r.Get(table.Id)).Returns(table);

            // Act + Assert
            Assert.That(() => _tableManager.StartGameForTable(table.Id),
                Throws.InvalidOperationException.With.Message.ContainsOne("not enough", "niet genoeg"));

            _tableRepositoryMock.Verify(repository => repository.Get(table.Id), Times.Once, "Table is not retrieved correctly");
            _gameRepositoryMock.Verify(r => r.Add(It.IsAny<IGame>()), Times.Never, "A game was added to the repository");
        }
    }
}
