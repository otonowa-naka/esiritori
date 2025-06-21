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
}
