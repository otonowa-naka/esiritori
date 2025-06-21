namespace EsiritoriApi.Api.Controllers;

using EsiritoriApi.Application.DTOs;
using EsiritoriApi.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public sealed class GamesController : ControllerBase
{
    private readonly ICreateGameUseCase _createGameUseCase;

    public GamesController(ICreateGameUseCase createGameUseCase)
    {
        _createGameUseCase = createGameUseCase ?? throw new ArgumentNullException(nameof(createGameUseCase));
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
}
