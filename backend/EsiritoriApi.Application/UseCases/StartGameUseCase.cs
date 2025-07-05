namespace EsiritoriApi.Application.UseCases;

using EsiritoriApi.Application.DTOs;
using EsiritoriApi.Application.Interfaces;
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

        return MapToResponse(game);
    }

    private static StartGameResponse MapToResponse(Game game)
    {
        return new StartGameResponse
        {
            Game = new GameDto
            {
                Id = game.Id.Value,
                Status = game.Status.ToString(),
                Settings = new GameSettingsDto
                {
                    TimeLimit = game.Settings.TimeLimit,
                    RoundCount = game.Settings.RoundCount,
                    PlayerCount = game.Settings.PlayerCount
                },
                CurrentRound = new RoundDto
                {
                    RoundNumber = game.CurrentRound.RoundNumber,
                    CurrentTurn = new TurnDto
                    {
                        TurnNumber = game.CurrentRound.CurrentTurn.TurnNumber,
                        Status = game.CurrentRound.CurrentTurn.Status.ToString(),
                        DrawerId = game.CurrentRound.CurrentTurn.DrawerId.Value,
                        Answer = game.CurrentRound.CurrentTurn.Answer.HasValue ? game.CurrentRound.CurrentTurn.Answer.Value.Value : string.Empty
                    }
                },
                Players = game.Players.Select(player => new PlayerDto
                {
                    Id = player.Id.Value,
                    Name = player.Name.Value,
                    IsReady = player.IsReady,
                    IsDrawer = player.IsDrawer
                }).ToList(),
                ScoreRecords = game.ScoreHistories.Select(score => new ScoreRecordDto
                {
                    PlayerId = score.PlayerId.Value,
                    RoundNumber = score.RoundNumber,
                    TurnNumber = score.TurnNumber,
                    Points = score.Points,
                    Reason = score.Reason.ToString(),
                    Timestamp = score.Timestamp
                }).ToList()
            }
        };
    }
} 