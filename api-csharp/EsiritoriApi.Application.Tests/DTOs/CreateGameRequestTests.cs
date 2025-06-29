namespace EsiritoriApi.Tests.Application.DTOs;

using EsiritoriApi.Application.DTOs;
using Xunit;

public sealed class CreateGameRequestTests
{
    [Fact]
    public void 有効な値でCreateGameRequestが正常に作成される()
    {
        var creatorName = "テストプレイヤー";
        var settings = new GameSettingsDto
        {
            TimeLimit = 60,
            RoundCount = 3,
            PlayerCount = 4
        };

        var request = new CreateGameRequest
        {
            CreatorName = creatorName,
            Settings = settings
        };

        Assert.Equal(creatorName, request.CreatorName);
        Assert.Equal(settings.TimeLimit, request.Settings.TimeLimit);
        Assert.Equal(settings.RoundCount, request.Settings.RoundCount);
        Assert.Equal(settings.PlayerCount, request.Settings.PlayerCount);
    }

    [Fact]
    public void デフォルト値でCreateGameRequestが正常に作成される()
    {
        var request = new CreateGameRequest();

        Assert.Equal(string.Empty, request.CreatorName);
        Assert.NotNull(request.Settings);
        Assert.Equal(60, request.Settings.TimeLimit);
        Assert.Equal(3, request.Settings.RoundCount);
        Assert.Equal(4, request.Settings.PlayerCount);
    }

    [Fact]
    public void プロパティが正常に設定される()
    {
        var request = new CreateGameRequest();

        request.CreatorName = "新しいプレイヤー";
        request.Settings = new GameSettingsDto
        {
            TimeLimit = 120,
            RoundCount = 5,
            PlayerCount = 6
        };

        Assert.Equal("新しいプレイヤー", request.CreatorName);
        Assert.Equal(120, request.Settings.TimeLimit);
        Assert.Equal(5, request.Settings.RoundCount);
        Assert.Equal(6, request.Settings.PlayerCount);
    }

    [Fact]
    public void StartGameRequest_プロパティが正しく設定される()
    {
        var request = new StartGameRequest
        {
            GameId = "123456"
        };

        Assert.Equal("123456", request.GameId);
    }

    [Fact]
    public void StartGameRequest_空のGameIdでも設定される()
    {
        var request = new StartGameRequest
        {
            GameId = ""
        };

        Assert.Equal("", request.GameId);
    }

    [Fact]
    public void StartGameRequest_nullのGameIdでも設定される()
    {
        var request = new StartGameRequest
        {
            GameId = null!
        };

        Assert.Null(request.GameId);
    }

    [Fact]
    public void StartGameResponse_プロパティが正しく設定される()
    {
        var game = new GameDto
        {
            Id = "123456",
            Status = "Playing",
            Settings = new GameSettingsDto
            {
                TimeLimit = 60,
                RoundCount = 3,
                PlayerCount = 4
            },
            CurrentRound = new RoundDto
            {
                RoundNumber = 1,
                CurrentTurn = new TurnDto
                {
                    TurnNumber = 1,
                    Status = "SettingAnswer",
                    DrawerId = "creator123",
                    Answer = ""
                }
            },
            Players = new List<PlayerDto>
            {
                new PlayerDto
                {
                    Id = "creator123",
                    Name = "作成者",
                    IsReady = true,
                    IsDrawer = true
                }
            },
            ScoreRecords = new List<ScoreRecordDto>()
        };

        var response = new StartGameResponse
        {
            Game = game
        };

        Assert.NotNull(response.Game);
        Assert.Equal("123456", response.Game.Id);
        Assert.Equal("Playing", response.Game.Status);
        Assert.Equal(60, response.Game.Settings.TimeLimit);
        Assert.Equal(3, response.Game.Settings.RoundCount);
        Assert.Equal(4, response.Game.Settings.PlayerCount);
        Assert.Equal(1, response.Game.CurrentRound.RoundNumber);
        Assert.Equal(1, response.Game.CurrentRound.CurrentTurn.TurnNumber);
        Assert.Equal("SettingAnswer", response.Game.CurrentRound.CurrentTurn.Status);
        Assert.Equal("creator123", response.Game.CurrentRound.CurrentTurn.DrawerId);
        Assert.Equal("", response.Game.CurrentRound.CurrentTurn.Answer);
        Assert.Single(response.Game.Players);
        Assert.Equal("creator123", response.Game.Players.First().Id);
        Assert.Equal("作成者", response.Game.Players.First().Name);
        Assert.True(response.Game.Players.First().IsReady);
        Assert.True(response.Game.Players.First().IsDrawer);
        Assert.Empty(response.Game.ScoreRecords);
    }

    [Fact]
    public void StartGameResponse_nullのGameでも設定される()
    {
        var response = new StartGameResponse
        {
            Game = null!
        };

        Assert.Null(response.Game);
    }
}
