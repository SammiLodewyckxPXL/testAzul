using AutoMapper;
using Azul.Api.Models.Input;
using Azul.Api.Models.Output;
using Azul.Core.GameAggregate.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Azul.Api.Controllers
{
    /// <summary>
    /// Provides game-play functionality.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ApiControllerBase
    {
        private readonly IGameService _gameService;
        private readonly IMapper _mapper;

        public GamesController(IGameService gameService, IMapper mapper)
        {
            _gameService = gameService;
            _mapper = mapper;
        }

        /// <summary>
        /// Gets information about a game
        /// </summary>
        /// <param name="id">Id (guid) of the game</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GameModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        public IActionResult GetGame(Guid id)
        {
            IGame game = _gameService.GetGame(id);
            GameModel gameModel = _mapper.Map<GameModel>(game);
            return Ok(gameModel);
        }

        /// <summary>
        /// Takes all the tiles of a type from a factory display or from the table center (which is a special kind of factory display).
        /// The tiles are taken by the player associated with the authenticated user.
        /// </summary>
        /// <param name="id">Id (guid) of the game</param>
        /// <param name="inputModel">Information about the move the player wants to make.</param>
        [HttpPost("{id}/take-tiles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        public IActionResult TakeTiles(Guid id, [FromBody] TakeTilesModel inputModel)
        {
            _gameService.TakeTilesFromFactory(id, UserId, inputModel.DisplayId, inputModel.TileType);
            return Ok();
        }

        /// <summary>
        /// Places the tiles the player has previously taken on a pattern line.
        /// </summary>
        /// <param name="id">Id (guid) of the game</param>
        /// <param name="inputModel">Information about the move the player wants to make.</param>
        [HttpPost("{id}/place-tiles-on-patternline")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        public IActionResult PlaceTilesOnPatternLine(Guid id, [FromBody] PlaceTilesModel inputModel)
        {
            _gameService.PlaceTilesOnPatternLine(id, UserId, inputModel.PatternLineIndex);
            return Ok();
        }

        /// <summary>
        /// Places the tiles the player has previously taken on the floor line.
        /// </summary>
        /// <param name="id">Id (guid) of the game</param>
        [HttpPost("{id}/place-tiles-on-floorline")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        public IActionResult PlaceTilesOnFloorLine(Guid id)
        {
            _gameService.PlaceTilesOnFloorLine(id, UserId);
            return Ok();
        }
    }
}
