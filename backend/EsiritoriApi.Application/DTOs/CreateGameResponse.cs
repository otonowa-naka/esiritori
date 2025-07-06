namespace EsiritoriApi.Application.DTOs;

using EsiritoriApi.Domain.Game.Entities;
using EsiritoriApi.Domain.Game;

public sealed class CreateGameResponse
{
    public GameDto Game { get; set; } = new();
    public PlayerDto Player { get; set; } = new();

    public static CreateGameResponse FromGame(Game game)
    {
        var creator = game.Players.First();

        return new CreateGameResponse
        {
            Game = GameDto.FromGame(game),
            Player = PlayerDto.FromPlayer(creator)
        };
    }
}

public sealed class GameDto
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public GameSettingsDto Settings { get; set; } = new();
    public RoundDto CurrentRound { get; set; } = new();
    public List<PlayerDto> Players { get; set; } = new();
    public List<ScoreRecordDto> ScoreRecords { get; set; } = new();

    public static GameDto FromGame(Game game)
    {
        return new GameDto
        {
            Id = game.Id.Value,
            Status = game.Status.ToString(),
            Settings = GameSettingsDto.FromGameSettings(game.Settings),
            CurrentRound = RoundDto.FromRound(game.CurrentRound),
            Players = game.Players.Select(PlayerDto.FromPlayer).ToList(),
            ScoreRecords = game.ScoreHistories.Select(ScoreRecordDto.FromScoreHistory).ToList()
        };
    }
}

public sealed class RoundDto
{
    public int RoundNumber { get; set; }
    public TurnDto CurrentTurn { get; set; } = new();

    public static RoundDto FromRound(EsiritoriApi.Domain.Game.ValueObjects.Round round)
    {
        return new RoundDto
        {
            RoundNumber = round.RoundNumber,
            CurrentTurn = TurnDto.FromTurn(round.CurrentTurn)
        };
    }
}

public sealed class TurnDto
{
    public int TurnNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public string DrawerId { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;

    public static TurnDto FromTurn(EsiritoriApi.Domain.Game.ValueObjects.Turn turn)
    {
        return new TurnDto
        {
            TurnNumber = turn.TurnNumber,
            Status = turn.Status.ToString(),
            DrawerId = turn.DrawerId.Value,
            Answer = turn.Answer.HasValue ? turn.Answer.Value.Value : string.Empty
        };
    }
}

public sealed class PlayerDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsReady { get; set; }
    public bool IsDrawer { get; set; }

    public static PlayerDto FromPlayer(EsiritoriApi.Domain.Game.Entities.Player player)
    {
        return new PlayerDto
        {
            Id = player.Id.Value,
            Name = player.Name.Value,
            IsReady = player.IsReady,
            IsDrawer = player.IsDrawer
        };
    }
}

public sealed class ScoreRecordDto
{
    public string PlayerId { get; set; } = string.Empty;
    public int RoundNumber { get; set; }
    public int TurnNumber { get; set; }
    public int Points { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }

    public static ScoreRecordDto FromScoreHistory(EsiritoriApi.Domain.Scoring.ValueObjects.ScoreHistory score)
    {
        return new ScoreRecordDto
        {
            PlayerId = score.PlayerId.Value,
            RoundNumber = score.RoundNumber,
            TurnNumber = score.TurnNumber,
            Points = score.Points,
            Reason = score.Reason.ToString(),
            Timestamp = score.Timestamp
        };
    }
}
