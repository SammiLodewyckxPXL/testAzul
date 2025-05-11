using Azul.Core.PlayerAggregate.Contracts;
using Azul.Core.TableAggregate;
using Azul.Core.TableAggregate.Contracts;
using Azul.Core.Tests.Extensions;
using Azul.Core.TileFactoryAggregate;
using Azul.Core.TileFactoryAggregate.Contracts;
using Guts.Client.Core;

namespace Azul.Core.Tests
{
    [ProjectComponentTestFixture("1TINProject", "Azul", "TileBag",
        @"Azul.Core\TileFactoryAggregate\TileBag.cs;")]
    public class TileBagTests
    {
        private ITileBag? _tileBag;

        [SetUp]
        public void BeforeEachTest()
        {
            _tileBag = new TileBag() as ITileBag;
        }

        [MonitoredTest]
        public void Class_ShouldBeInternal_SoThatItCanOnlyBeUsedInTheCoreProject()
        {
            Assert.That(typeof(TileBag).IsNotPublic, Is.True, "use 'internal class' instead of 'public class'");
        }

        [MonitoredTest]
        public void Class_ShouldImplement_ITileBag()
        {
            Assert.That(typeof(TileBag).IsAssignableTo(typeof(ITileBag)), Is.True);
        }

        [MonitoredTest]
        public void ITileBag_Interface_ShouldHaveCorrectMembers()
        {
            var type = typeof(ITileBag);

            type.AssertInterfaceProperty(nameof(ITileBag.Tiles), shouldHaveGetter: true, shouldHaveSetter: false);

            type.AssertInterfaceMethod(nameof(ITileBag.AddTiles), typeof(void), typeof(int), typeof(TileType));
            type.AssertInterfaceMethod(nameof(ITileBag.AddTiles), typeof(void), typeof(IReadOnlyList<TileType>));
            type.AssertInterfaceMethod(nameof(ITileBag.TryTakeTiles), typeof(bool), typeof(int), typeof(IReadOnlyList<TileType>));
        }

        [MonitoredTest]
        public void AddTiles_AmountAndType_ShouldAddCorrectAmountOfTiles()
        {
            Assert.That(_tileBag, Is.Not.Null, "The tile bag should implement ITileBag.");

            // Arrange
            int amount = 10;
            TileType tileType = Random.Shared.NextTileType();

            // Act
            _tileBag!.AddTiles(amount, tileType);

            // Assert
            Assert.That(_tileBag.Tiles.Count(t => t == tileType), Is.EqualTo(amount));
        }

        [MonitoredTest]
        public void AddTiles_ListOfTiles_ShouldAddTilesInTheList()
        {
            Assert.That(_tileBag, Is.Not.Null, "The tile bag should implement ITileBag.");

            // Arrange
            var tilesToAdd = new List<TileType>();
            for (int i = 0; i < Random.Shared.Next(3,11); i++)
            {
                tilesToAdd.Add(Random.Shared.NextTileType());
            }

            // Act
            _tileBag!.AddTiles(tilesToAdd);

            // Assert
            Assert.That(_tileBag.Tiles, Is.EquivalentTo(tilesToAdd));
        }

        [MonitoredTest]
        public void TryTakeTiles_NotEnoughTiles_ShouldReturnFalse()
        {
            Assert.That(_tileBag, Is.Not.Null, "The tile bag should implement ITileBag.");

            // Arrange
            int n = Random.Shared.Next(3,11);
            _tileBag!.AddTiles(n, TileType.PlainBlue);

            // Act
            bool result = _tileBag.TryTakeTiles(n + 1, out var takenTiles);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(takenTiles.Count, Is.EqualTo(n),
                "All the tiles should be in the list of tiles that were taken");
            Assert.That(_tileBag.Tiles, Is.Empty,
                "The bag should be empty after trying to take more tiles than present in the bag");
        }

        [MonitoredTest]
        public void TryTakeTiles_EnoughTiles_ShouldReturnTrue()
        {
            Assert.That(_tileBag, Is.Not.Null, "The tile bag should implement ITileBag.");

            // Arrange
            int numberOfTilesInBag = Random.Shared.Next(4, 11);
            _tileBag!.AddTiles(numberOfTilesInBag, TileType.PlainBlue);

            int numberOfTilesToTake = Random.Shared.Next(1, numberOfTilesInBag + 1);
            int expectedRemainingTiles = numberOfTilesInBag - numberOfTilesToTake;

            // Act
            bool result = _tileBag.TryTakeTiles(numberOfTilesToTake, out var takenTiles);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(takenTiles.Count, Is.EqualTo(numberOfTilesToTake),
                $"There should be {numberOfTilesToTake} tiles in the list of tiles that were taken after trying to take {numberOfTilesToTake} tiles");
            Assert.That(_tileBag.Tiles.Count, Is.EqualTo(expectedRemainingTiles),
                $"There should be {expectedRemainingTiles} tiles left in the bag " +
                $"after taking {numberOfTilesToTake} from a bag that contains {numberOfTilesInBag} tiles");
        }

        [MonitoredTest]
        public void TryTakeTiles_ShouldTakeRandomTiles()
        {
            int numberOfTakes = 50;
            int numberOfDifferentTakes = 0;
            
            var previousTakenTiles = new List<TileType>();
            for (int i = 0; i < numberOfTakes; i++)
            {
                _tileBag = new TileBag() as ITileBag;
                Assert.That(_tileBag, Is.Not.Null, "The tile bag should implement ITileBag.");
                _tileBag.AddTiles(20, TileType.PlainBlue);
                _tileBag.AddTiles(20, TileType.WhiteTurquoise);
                _tileBag.AddTiles(20, TileType.PlainRed);
                _tileBag.AddTiles(20, TileType.BlackBlue);
                _tileBag.AddTiles(20, TileType.YellowRed);
                
                bool result = _tileBag.TryTakeTiles(4, out var takenTiles);

                Assert.That(result, Is.True, "Failed to take tiles");

                if(takenTiles.Any(t => !previousTakenTiles.Contains(t)))
                {
                    numberOfDifferentTakes++;
                }

                previousTakenTiles = takenTiles.ToList();
            }

            int percentageDifferent = (numberOfDifferentTakes * 100) / numberOfTakes;
            Assert.That(percentageDifferent, Is.GreaterThan(60),
                "Taking tiles does not seem random enough. " +
                $"After taking tiles {numberOfTakes} times, " +
                $"only {percentageDifferent}% yields a different tile combination that the previous take");
        }
    }
}
