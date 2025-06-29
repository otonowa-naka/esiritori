namespace EsiritoriApi.Application.UseCases;

using EsiritoriApi.Application.DTOs;
using EsiritoriApi.Application.Interfaces;
using EsiritoriApi.Domain.Game;
using EsiritoriApi.Domain.Game.Entities;
using EsiritoriApi.Domain.Game.ValueObjects;
using EsiritoriApi.Domain.Scoring.ValueObjects;

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

        var gameId = GameId.NewId();
        var playerId = PlayerId.NewId();
        var creatorName = new PlayerName(request.CreatorName);
        var settings = new GameSettings(
            request.Settings.TimeLimit,
            request.Settings.RoundCount,
            request.Settings.PlayerCount
        );

        var creator = new Player(playerId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit);
        var initialRound = Round.CreateInitial(initialTurn, DateTime.UtcNow);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), DateTime.UtcNow, DateTime.UtcNow);

        await _gameRepository.SaveAsync(game, cancellationToken);

        return MapToResponse(game);
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
