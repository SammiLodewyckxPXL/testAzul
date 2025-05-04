using Azul.Core.TableAggregate;
using Azul.Core.TableAggregate.Contracts;
using Azul.Core.Tests.Builders;
using Azul.Core.UserAggregate;
using Guts.Client.Core;

namespace Azul.Infrastructure.Tests
{
    [ProjectComponentTestFixture("1TINProject", "Azul", "InMemoryTableRepo",
        @"Azul.Infrastructure\InMemoryTableRepository.cs")]
    public class InMemoryTableRepositoryTests
    {
        private InMemoryTableRepository _repository = null!;

        [SetUp]
        public void Setup()
        {
            _repository = new InMemoryTableRepository();
        }

        [MonitoredTest]
        public void FindTablesWithAvailableSeats_SomeTablesHaveAvailableSeats_ShouldReturnThem()
        {
            //Arrange
            ITablePreferences preferences = new TablePreferencesBuilder().Build();
            int numberOfTablesWithOnePlayer = Random.Shared.Next(1, 11);
            for (int i = 0; i < numberOfTablesWithOnePlayer; i++)
            {
                AddTableWithOnePlayer(preferences);
            }
            int numberOfFullTables = Random.Shared.Next(1, 11);
            for (int i = 0; i < numberOfFullTables; i++)
            {
                AddFullTable(preferences);
            }
            int numberOfTablesWithMismatchingPreferences = Random.Shared.Next(1, 11);
            ITablePreferences otherPreferences = new TablePreferencesBuilder().WithNumberOfPlayers(3).Build();
            for (int i = 0; i < numberOfTablesWithMismatchingPreferences; i++)
            {
                AddTableWithOnePlayer(otherPreferences);
            }

            //Act
            IList<ITable>? results = _repository.FindTablesWithAvailableSeats(preferences);

            //Assert
            Assert.That(results, Is.Not.Null, "No list was returned.");
            foreach (ITable table in results)
            {
                Assert.That(table.HasAvailableSeat, Is.True, "Not all returned tables have seats available");
            }
            Assert.That(results.Count, Is.EqualTo(numberOfTablesWithOnePlayer), "The number of returned tables is incorrect.");
        }

        [MonitoredTest]
        public void FindTablesWithAvailableSeats_AllTablesAreFull_ShouldReturnAnEmptyList()
        {
            //Arrange
            ITablePreferences preferences = new TablePreferencesBuilder().Build();
            int numberOfFullTables = Random.Shared.Next(3, 11);
            for (int i = 0; i < numberOfFullTables; i++)
            {
                AddFullTable(preferences);
            }

            //Act
            IList<ITable>? results = _repository.FindTablesWithAvailableSeats(preferences);

            //Assert
            Assert.That(results, Is.Not.Null, "No list was returned.");
            Assert.That(results, Is.Empty, "The list returned should be empty.");
        }

        private void AddTableWithOnePlayer(ITablePreferences preferences)
        {
            _repository.Add(new TableMockBuilder().WithPreferences(preferences).WithSeatedUsers([new UserBuilder().Build()]).Object);
        }

        private void AddFullTable(ITablePreferences preferences)
        {
            var users = new List<User>();
            for (int i = 0; i < preferences.NumberOfPlayers; i++)
            {
                users.Add(new UserBuilder().Build());
            }
            _repository.Add(new TableMockBuilder().WithSeatedUsers(users.ToArray()).Object);
        }
    }
}