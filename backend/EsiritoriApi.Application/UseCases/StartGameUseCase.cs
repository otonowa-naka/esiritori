namespace EsiritoriApi.Application.UseCases;

using EsiritoriApi.Application.DTOs;
using EsiritoriApi.Domain.Game;
using EsiritoriApi.Domain.Game.ValueObjects;

public interface IStartGameUseCase
{
    Task<StartGameResponse> ExecuteAsync(StartGameRequest request, CancellationToken cancellationToken = default);
}

public class StartGameUseCase : IStartGameUseCase
{
    private readonly IGameRepository _gameRepository;

    public StartGameUseCase(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
    }

    public async Task<StartGameResponse> ExecuteAsync(StartGameRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var gameId = new GameId(request.GameId);
        var game = await _gameRepository.FindByIdAsync(gameId, cancellationToken);
        
        if (game == null)
        {
            throw new InvalidOperationException("指定されたゲームが見つかりません");
        }

        // ゲームを開始し、新規Roundを作成
        game.StartGame(DateTime.UtcNow);

        await _gameRepository.SaveAsync(game, cancellationToken);

        return StartGameResponse.FromGame(game);
    }

} 