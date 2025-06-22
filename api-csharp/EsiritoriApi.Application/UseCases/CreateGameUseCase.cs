namespace EsiritoriApi.Application.UseCases;

using EsiritoriApi.Application.DTOs;
using EsiritoriApi.Application.Interfaces;
using EsiritoriApi.Domain.Entities;
using EsiritoriApi.Domain.ValueObjects;

public interface ICreateGameUseCase
{
    Task<CreateGameResponse> ExecuteAsync(CreateGameRequest request, CancellationToken cancellationToken = default);
}

public class CreateGameUseCase : ICreateGameUseCase
{
    private readonly IGameRepository _gameRepository;

    public CreateGameUseCase(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
    }

    public async Task<CreateGameResponse> ExecuteAsync(CreateGameRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var gameId = new GameId(GenerateGameId());
        var playerId = new PlayerId(GeneratePlayerId());
        var creatorName = new PlayerName(request.CreatorName);
        var settings = new GameSettings(
            request.Settings.TimeLimit,
            request.Settings.RoundCount,
            request.Settings.PlayerCount
        );

        var game = new Game(gameId, settings, creatorName, playerId);

        await _gameRepository.SaveAsync(game, cancellationToken);

        return MapToResponse(game);
    }

    private static string GenerateGameId()
    {
        return Random.Shared.Next(100000, 999999).ToString();
    }

    private static string GeneratePlayerId()
    {
        return Guid.NewGuid().ToString("N")[..12];
    }

    private static CreateGameResponse MapToResponse(Game game)
    {
        var creator = game.Players.First();

        return new CreateGameResponse
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
                        Answer = game.CurrentRound.CurrentTurn.Answer.Value
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
            },
            Player = new PlayerDto
            {
                Id = creator.Id.Value,
                Name = creator.Name.Value,
                IsReady = creator.IsReady,
                IsDrawer = creator.IsDrawer
            }
        };
    }
}
