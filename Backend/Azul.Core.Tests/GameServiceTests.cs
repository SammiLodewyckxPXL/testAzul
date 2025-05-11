using System;
using Moq;
using NUnit.Framework;
using Azul.Core.GameAggregate;
using Azul.Core.GameAggregate.Contracts;
using Azul.Core.PlayerAggregate.Contracts;
using Azul.Core.Tests.Extensions;
using Azul.Core.TileFactoryAggregate.Contracts;
using Guts.Client.Core;

namespace Azul.Core.Tests
{
    [ProjectComponentTestFixture("1TINProject", "Azul", "GameService",
        @"Azul.Core\GameAggregate\GameService.cs;")]
    public class GameServiceTests
    {
        private Mock<IGameRepository> _gameRepositoryMock = null!;
        private GameService _gameService = null!;
        private Guid _gameId;
        private Guid _playerId;
        private Guid _displayId;
        private TileType _tileType;

        [SetUp]
        public void SetUp()
        {
            _gameRepositoryMock = new Mock<IGameRepository>();
            _gameService = new GameService(_gameRepositoryMock.Object);
            _gameId = Guid.NewGuid();
            _playerId = Guid.NewGuid();
            _displayId = Guid.NewGuid();
            _tileType = Random.Shared.NextTileType();
        }

        [MonitoredTest]
        public void GetGame_ShouldUseRepositoryToRetrieveGame()
        {
            // Arrange
            var gameMock = new Mock<IGame>();
            _gameRepositoryMock.Setup(repo => repo.GetById(_gameId)).Returns(gameMock.Object);

            // Act
            IGame? result = _gameService.GetGame(_gameId);

            // Assert
            Assert.That(gameMock.Object, Is.SameAs(result));
        }

        [MonitoredTest]
        public void TakeTilesFromFactory_ShouldUseRepositoryToRetrieveGame_ShouldCallTakeTilesFromFactoryOnGame()
        {
            // Arrange
            var gameMock = new Mock<IGame>();
            _gameRepositoryMock.Setup(repo => repo.GetById(_gameId)).Returns(gameMock.Object);

            // Act
            _gameService.TakeTilesFromFactory(_gameId, _playerId, _displayId, _tileType);

            // Assert
            gameMock.Verify(game => game.TakeTilesFromFactory(_playerId, _displayId, _tileType), Times.Once);
        }

        [MonitoredTest]
        public void PlaceTilesOnPatternLine_ShouldUseRepositoryToRetrieveGame_ShouldCallPlaceTilesOnPatternLineOnGame()
        {
            // Arrange
            var gameMock = new Mock<IGame>();
            _gameRepositoryMock.Setup(repo => repo.GetById(_gameId)).Returns(gameMock.Object);
            int patternLineIndex = Random.Shared.Next(5);

            // Act
            _gameService.PlaceTilesOnPatternLine(_gameId, _playerId, patternLineIndex);

            // Assert
            gameMock.Verify(game => game.PlaceTilesOnPatternLine(_playerId, patternLineIndex), Times.Once);
        }

        [MonitoredTest]
        public void PlaceTilesOnFloorLine_ShouldUseRepositoryToRetrieveGame_ShouldCallPlaceTilesOnFloorLineOnGame()
        {
            // Arrange
            var gameMock = new Mock<IGame>();
            _gameRepositoryMock.Setup(repo => repo.GetById(_gameId)).Returns(gameMock.Object);

            // Act
            _gameService.PlaceTilesOnFloorLine(_gameId, _playerId);

            // Assert
            gameMock.Verify(game => game.PlaceTilesOnFloorLine(_playerId), Times.Once);
        }
    }
}
