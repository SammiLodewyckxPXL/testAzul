using System.Reflection;
using Azul.Core.BoardAggregate;
using Azul.Core.Tests.Extensions;
using Azul.Core.TileFactoryAggregate.Contracts;
using Guts.Client.Core;
using NUnit.Framework;

namespace Azul.Core.Tests
{
    [ProjectComponentTestFixture("1TINProject", "Azul", "TileSpot",
        @"Azul.Core\BoardAggregate\TileSpot.cs;")]
    public class TileSpotTests
    {
        private TileSpot _tileSpot = null!;

        [SetUp]
        public void SetUp()
        {
            _tileSpot = new TileSpot();
        }

        [MonitoredTest]
        public void Constructor_TypeIsNull_ShouldCorrectlyInitializeProperties()
        {
            // Arrange & Act
            var tileSpot = new TileSpot();

            // Assert
            Assert.That(tileSpot.Type, Is.Null, "Type should be null after initialization.");
            Assert.That(tileSpot.HasTile, Is.False, "HasTile should be false after initialization.");
        }

        [MonitoredTest]
        public void Constructor_WithType_ShouldInitializeProperties()
        {
            // Arrange
            TileType tileType = Random.Shared.NextTileType();

            // Act
            var tileSpot = new TileSpot(tileType);

            // Assert
            Assert.That(tileSpot.Type, Is.EqualTo(tileType), "Type should be initialized to the provided value.");
            Assert.That(tileSpot.HasTile, Is.False, "HasTile should be false after initialization.");
        }

        [MonitoredTest]
        public void Properties_ShouldHavePrivateSetters()
        {
            AssertHasPrivateSetter(nameof(TileSpot.Type));
            AssertHasPrivateSetter(nameof(TileSpot.HasTile));
        }

        private static void AssertHasPrivateSetter(string propertyName)
        {
            PropertyInfo property = typeof(TileSpot).GetProperty(propertyName)!;
            Assert.That(property.SetMethod, Is.Not.Null, $"{property.Name} should have a setter.");
            Assert.That(property.SetMethod!.IsPrivate, Is.True, $"The setter of {property.Name} should be private.");
        }

        [MonitoredTest]
        public void PlaceTile_ShouldSetTypeAndHasTile()
        {
            // Arrange
            TileType tileType = Random.Shared.NextTileType();

            // Act
            _tileSpot.PlaceTile(tileType);

            // Assert
            Assert.That(_tileSpot.Type, Is.EqualTo(tileType), "Type should be set to the provided value.");
            Assert.That(_tileSpot.HasTile, Is.True, "HasTile should be true after placing a tile.");
        }

        [MonitoredTest]
        public void PlaceTile_AlreadyHasTile_ShouldThrowInvalidOperationException()
        {
            // Arrange
            TileType tileType = Random.Shared.NextTileType();
            _tileSpot.PlaceTile(tileType);

            // Act & Assert
            Assert.That(() => _tileSpot.PlaceTile(tileType), Throws.InvalidOperationException.With.Message.ContainsOne("already", "reeds"));
        }

        [MonitoredTest]
        public void PlaceTile_DifferentType_ShouldThrowInvalidOperationException()
        {
            // Arrange
            TileType initialType = TileType.PlainBlue;
            TileType newType = TileType.PlainRed;
            _tileSpot = new TileSpot(initialType);

            // Act & Assert
            Assert.That(() => _tileSpot.PlaceTile(newType), Throws.InvalidOperationException.With.Message.ContainsOne("type"));
        }

        [MonitoredTest]
        public void Clear_ShouldResetProperties()
        {
            // Arrange
            TileType tileType = Random.Shared.NextTileType();
            _tileSpot.PlaceTile(tileType);

            // Act
            _tileSpot.Clear();

            // Assert
            Assert.That(_tileSpot.Type, Is.Null, "Type should be null after clearing.");
            Assert.That(_tileSpot.HasTile, Is.False, "HasTile should be false after clearing.");
        }
    }
}
