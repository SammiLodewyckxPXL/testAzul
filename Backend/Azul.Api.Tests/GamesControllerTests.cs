using System.Security.Claims;
using AutoMapper;
using Azul.Api.Controllers;
using Azul.Api.Models.Input;
using Azul.Api.Models.Output;
using Azul.Core.GameAggregate.Contracts;
using Azul.Core.Tests.Builders;
using Azul.Core.Tests.Extensions;
using Azul.Core.TileFactoryAggregate.Contracts;
using Azul.Core.UserAggregate;
using Guts.Client.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Azul.Api.Tests
{
    [ProjectComponentTestFixture("1TINProject", "Azul", "GamesCtlr",
        @"Azul.Api\Controllers\GamesController.cs")]
    public class GamesControllerTests
    {
        private Mock<IGameService> _gameServiceMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private GamesController _controller = null!;
        private User _loggedInUser = null!;

        [SetUp]
        public void SetUp()
        {
            _gameServiceMock = new Mock<IGameService>();


            _mapperMock = new Mock<IMapper>();
            _controller = new GamesController(_gameServiceMock.Object, _mapperMock.Object);

            _loggedInUser = new UserBuilder().Build();
            var userClaimsPrincipal = new ClaimsPrincipal(
                new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, _loggedInUser.Id.ToString())
                })
            );
            var context = new ControllerContext { HttpContext = new DefaultHttpContext() };
            context.HttpContext.User = userClaimsPrincipal;
            _controller.ControllerContext = context;
        }

        [MonitoredTest]
        public void GetGame_ShouldUseServiceToRetrieveGame()
        {
            // Arrange
            IGame game = new GameMockBuilder().Object;
            var gameModel = new GameModel();
            _gameServiceMock.Setup(s => s.GetGame(game.Id)).Returns(game);
            _mapperMock.Setup(m => m.Map<GameModel>(game)).Returns(gameModel);

            // Act
            var result = _controller.GetGame(game.Id) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null, "An instance of 'OkObjectResult' should be returned.");
            _mapperMock.Verify(mapper => mapper.Map<GameModel>(game), Times.Once,
                "The game is not correctly mapped to a game model");
            Assert.That(result!.Value, Is.SameAs(gameModel), "The mapped game model is not in the OkObjectResult");
        }

        [MonitoredTest]
        public void TakeTiles_ShouldUseService()
        {
            // Arrange
            Guid gameId = Guid.NewGuid();
            var inputModel = new TakeTilesModel()
            {
                DisplayId = Guid.NewGuid(),
                TileType = Random.Shared.NextTileType()
            };

            // Act
            var result = _controller.TakeTiles(gameId, inputModel) as OkResult;

            // Assert
            Assert.That(result, Is.Not.Null, "An instance of 'OkResult' should be returned.");
            _gameServiceMock.Verify(service => service.TakeTilesFromFactory(gameId, _loggedInUser.Id, inputModel.DisplayId, inputModel.TileType), Times.Once,
                               "The service is not called with the correct parameters");
        }

        [MonitoredTest]
        public void PlaceTilesOnPatternLine_ShouldUseService()
        {
            // Arrange
            Guid gameId = Guid.NewGuid();
            var inputModel = new PlaceTilesModel()
            {
                PatternLineIndex = Random.Shared.Next(0, 5)
            };

            // Act
            var result = _controller.PlaceTilesOnPatternLine(gameId, inputModel) as OkResult;

            // Assert
            Assert.That(result, Is.Not.Null, "An instance of 'OkResult' should be returned.");
            _gameServiceMock.Verify(service => service.PlaceTilesOnPatternLine(gameId, _loggedInUser.Id, inputModel.PatternLineIndex), Times.Once,
                "The service is not called with the correct parameters");
        }

        [MonitoredTest]
        public void PlaceTilesOnFloorLine_ShouldUseService()
        {
            // Arrange
            Guid gameId = Guid.NewGuid();

            // Act
            var result = _controller.PlaceTilesOnFloorLine(gameId) as OkResult;

            // Assert
            Assert.That(result, Is.Not.Null, "An instance of 'OkResult' should be returned.");
            _gameServiceMock.Verify(service => service.PlaceTilesOnFloorLine(gameId, _loggedInUser.Id), Times.Once,
                "The service is not called with the correct parameters");
        }
    }
}
