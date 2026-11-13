using BeatTheRocky.Api.DTOs;
using BeatTheRocky.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BeatTheRocky.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly ILogger<GameController> _logger;

    public GameController(IGameService gameService, ILogger<GameController> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a brand new game session.
    /// </summary>
    [HttpPost("new")]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    public ActionResult<GameResponse> CreateGame()
    {
        var session = _gameService.CreateSession();
        return Ok(GameResponse.FromSession(session));
    }

    /// <summary>
    /// Performs the coin toss to decide who makes the first move.
    /// </summary>
    [HttpPost("{id:guid}/toss")]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<GameResponse> TossCoin(Guid id, [FromBody] CoinTossRequest request)
    {
        try
        {
            var session = _gameService.ProcessToss(id, request.Choice);
            return Ok(GameResponse.FromSession(session));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing toss for {SessionId}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Submits the player's move on the board (cell index 0-8). Rocky will automatically respond.
    /// </summary>
    [HttpPost("{id:guid}/move")]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<GameResponse> MakeMove(Guid id, [FromBody] MakeMoveRequest request)
    {
        try
        {
            var session = _gameService.MakeMove(id, request.CellIndex);
            return Ok(GameResponse.FromSession(session));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making move for {SessionId}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves current state of the game session.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<GameResponse> GetGame(Guid id)
    {
        var session = _gameService.GetSession(id);
        if (session == null)
        {
            return NotFound(new { error = $"Session {id} not found." });
        }
        return Ok(GameResponse.FromSession(session));
    }

    /// <summary>
    /// Resets the game session for a rematch.
    /// </summary>
    [HttpPost("{id:guid}/reset")]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    public ActionResult<GameResponse> ResetGame(Guid id)
    {
        var session = _gameService.ResetSession(id);
        return Ok(GameResponse.FromSession(session));
    }
}
