namespace EsiritoriApi.Application.Tests.DTOs;

using EsiritoriApi.Application.DTOs;
using Xunit;

[Trait("Category", "ユースケース")]
public sealed class StartGameResponseTests
{
    [Fact]
    public void StartGameResponse作成時_デフォルト値が設定される()
    {
        var response = new StartGameResponse();

        Assert.NotNull(response.Game);
    }

    [Fact]
    public void StartGameResponse_Gameプロパティの設定と取得()
    {
        var response = new StartGameResponse();
        var gameDto = new GameDto
        {
            Id = "123456",
            Status = "Playing",
            Settings = new GameSettingsDto { TimeLimit = 60, RoundCount = 3, PlayerCount = 4 },
            CurrentRound = new RoundDto 
            { 
                RoundNumber = 1,
                CurrentTurn = new TurnDto 
                { 
                    TurnNumber = 1, 
                    Status = "Drawing", 
                    DrawerId = "player1",
                    Answer = ""
                }
            },
            Players = new List<PlayerDto>(),
            ScoreRecords = new List<ScoreRecordDto>()
        };

        response.Game = gameDto;

        Assert.Equal(gameDto, response.Game);
        Assert.Equal("123456", response.Game.Id);
        Assert.Equal("Playing", response.Game.Status);
    }

    [Fact]
    public void StartGameResponse_GameDtoの詳細プロパティ検証()
    {
        var response = new StartGameResponse();
        var players = new List<PlayerDto>
        {
            new() { Id = "player1", Name = "テストプレイヤー1", IsReady = true, IsDrawer = true },
            new() { Id = "player2", Name = "テストプレイヤー2", IsReady = true, IsDrawer = false }
        };
        var scoreRecords = new List<ScoreRecordDto>
        {
            new() { PlayerId = "player1", RoundNumber = 1, TurnNumber = 1, Points = 10, Reason = "Drawing", Timestamp = DateTime.Now }
        };

        response.Game = new GameDto
        {
            Id = "game123",
            Status = "Playing",
            Settings = new GameSettingsDto { TimeLimit = 90, RoundCount = 5, PlayerCount = 2 },
            CurrentRound = new RoundDto 
            { 
                RoundNumber = 2,
                CurrentTurn = new TurnDto 
                { 
                    TurnNumber = 3, 
                    Status = "Guessing", 
                    DrawerId = "player2",
                    Answer = "猫"
                }
            },
            Players = players,
            ScoreRecords = scoreRecords
        };

        Assert.Equal("game123", response.Game.Id);
        Assert.Equal("Playing", response.Game.Status);
        Assert.Equal(90, response.Game.Settings.TimeLimit);
        Assert.Equal(5, response.Game.Settings.RoundCount);
        Assert.Equal(2, response.Game.Settings.PlayerCount);
        Assert.Equal(2, response.Game.CurrentRound.RoundNumber);
        Assert.Equal(3, response.Game.CurrentRound.CurrentTurn.TurnNumber);
        Assert.Equal("Guessing", response.Game.CurrentRound.CurrentTurn.Status);
        Assert.Equal("player2", response.Game.CurrentRound.CurrentTurn.DrawerId);
        Assert.Equal("猫", response.Game.CurrentRound.CurrentTurn.Answer);
        Assert.Equal(2, response.Game.Players.Count);
        Assert.Equal(1, response.Game.ScoreRecords.Count);
    }

    [Fact]
    public void StartGameResponse_null設定時()
    {
        var response = new StartGameResponse { Game = null! };

        Assert.Null(response.Game);
    }
}