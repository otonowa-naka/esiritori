namespace EsiritoriApi.Tests.Application.DTOs;

using EsiritoriApi.Application.DTOs;
using Xunit;

public sealed class CreateGameResponseTests
{
    [Fact]
    public void 有効な値でCreateGameResponseが正常に作成される()
    {
        var gameId = "game123";
        var playerName = "テストプレイヤー";
        var playerId = "player123";

        var response = new CreateGameResponse
        {
            Game = new GameDto
            {
                Id = gameId,
                Status = "Waiting",
                Settings = new GameSettingsDto
                {
                    TimeLimit = 60,
                    RoundCount = 3,
                    PlayerCount = 4
                },
                Players = new List<PlayerDto>
                {
                    new PlayerDto
                    {
                        Id = playerId,
                        Name = playerName,
                        IsReady = false,
                        IsDrawer = false
                    }
                }
            },
            Player = new PlayerDto
            {
                Id = playerId,
                Name = playerName,
                IsReady = false,
                IsDrawer = false
            }
        };

        Assert.Equal(gameId, response.Game.Id);
        Assert.Equal("Waiting", response.Game.Status);
        Assert.Equal(60, response.Game.Settings.TimeLimit);
        Assert.Equal(3, response.Game.Settings.RoundCount);
        Assert.Equal(4, response.Game.Settings.PlayerCount);
        Assert.Single(response.Game.Players);
        Assert.Equal(playerName, response.Game.Players.First().Name);
        Assert.Equal(playerId, response.Player.Id);
        Assert.Equal(playerName, response.Player.Name);
    }

    [Fact]
    public void GameDtoが正常に作成される()
    {
        var game = new GameDto
        {
            Id = "game456",
            Status = "Playing",
            Settings = new GameSettingsDto
            {
                TimeLimit = 90,
                RoundCount = 5,
                PlayerCount = 8
            }
        };

        Assert.Equal("game456", game.Id);
        Assert.Equal("Playing", game.Status);
        Assert.Equal(90, game.Settings.TimeLimit);
        Assert.Equal(5, game.Settings.RoundCount);
        Assert.Equal(8, game.Settings.PlayerCount);
    }

    [Fact]
    public void PlayerDtoが正常に作成される()
    {
        var player = new PlayerDto
        {
            Id = "player456",
            Name = "テストプレイヤー2",
            IsReady = true,
            IsDrawer = true
        };

        Assert.Equal("player456", player.Id);
        Assert.Equal("テストプレイヤー2", player.Name);
        Assert.True(player.IsReady);
        Assert.True(player.IsDrawer);
    }

    [Fact]
    public void 空のプレイヤーリストでGameDtoが作成される()
    {
        var game = new GameDto
        {
            Id = "game789",
            Status = "Waiting",
            Settings = new GameSettingsDto(),
            Players = new List<PlayerDto>()
        };

        Assert.Equal("game789", game.Id);
        Assert.Equal("Waiting", game.Status);
        Assert.Empty(game.Players);
    }

    [Fact]
    public void RoundDtoが正常に作成される()
    {
        var round = new RoundDto
        {
            RoundNumber = 2,
            CurrentTurn = new TurnDto
            {
                TurnNumber = 1,
                Status = "Drawing",
                DrawerId = "player123",
                Answer = ""
            }
        };

        Assert.Equal(2, round.RoundNumber);
        Assert.Equal(1, round.CurrentTurn.TurnNumber);
        Assert.Equal("Drawing", round.CurrentTurn.Status);
        Assert.Equal("player123", round.CurrentTurn.DrawerId);
    }

    [Fact]
    public void ScoreRecordDtoが正常に作成される()
    {
        var timestamp = DateTime.UtcNow;
        var scoreRecord = new ScoreRecordDto
        {
            PlayerId = "player123",
            RoundNumber = 1,
            TurnNumber = 1,
            Points = 100,
            Reason = "CorrectAnswer",
            Timestamp = timestamp
        };

        Assert.Equal("player123", scoreRecord.PlayerId);
        Assert.Equal(1, scoreRecord.RoundNumber);
        Assert.Equal(1, scoreRecord.TurnNumber);
        Assert.Equal(100, scoreRecord.Points);
        Assert.Equal("CorrectAnswer", scoreRecord.Reason);
        Assert.Equal(timestamp, scoreRecord.Timestamp);
    }
}
