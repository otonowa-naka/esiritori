namespace EsiritoriApi.Api.Controllers;

using EsiritoriApi.Application.DTOs;
using EsiritoriApi.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public sealed class GamesController : ControllerBase
{
    private readonly ICreateGameUseCase _createGameUseCase;
    private readonly IStartGameUseCase _startGameUseCase;

    public GamesController(ICreateGameUseCase createGameUseCase, IStartGameUseCase startGameUseCase)
    {
        _createGameUseCase = createGameUseCase ?? throw new ArgumentNullException(nameof(createGameUseCase));
        _startGameUseCase = startGameUseCase ?? throw new ArgumentNullException(nameof(startGameUseCase));
    }

    [HttpPost]
    public async Task<ActionResult<CreateGameResponse>> CreateGame(
        [FromBody] CreateGameRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.CreatorName))
            {
                return BadRequest(new { error = "プレイヤー名は必須です" });
            }

            var response = await _createGameUseCase.ExecuteAsync(request, cancellationToken);
            return CreatedAtAction(nameof(CreateGame), new { id = response.Game.Id }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "内部サーバーエラーが発生しました", details = ex.Message });
        }
    }

    [HttpPost("{gameId}/start")]
    public async Task<ActionResult<StartGameResponse>> StartGame(
        string gameId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(gameId))
            {
                return BadRequest(new { error = "ゲームIDは必須です" });
            }

            var request = new StartGameRequest { GameId = gameId };
            var response = await _startGameUseCase.ExecuteAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message == "指定されたゲームが見つかりません")
            {
                return BadRequest(new { error = ex.Message });
            }
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "内部サーバーエラーが発生しました", details = ex.Message });
        }
    }
}
