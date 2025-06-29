namespace EsiritoriApi.Application.Tests.DTOs;

using EsiritoriApi.Application.DTOs;
using Xunit;

[Trait("Category", "ユースケース")]
public sealed class StartGameRequestTests
{
    [Fact]
    public void StartGameRequest作成時_プロパティが正常に設定される()
    {
        var gameId = "123456";
        var request = new StartGameRequest { GameId = gameId };

        Assert.Equal(gameId, request.GameId);
    }

    [Fact]
    public void StartGameRequest作成時_デフォルト値が設定される()
    {
        var request = new StartGameRequest();

        Assert.Equal(string.Empty, request.GameId);
    }

    [Fact]
    public void StartGameRequest_GameIdプロパティの設定と取得()
    {
        var request = new StartGameRequest();
        var testGameId = "TEST_GAME_ID_12345";

        request.GameId = testGameId;

        Assert.Equal(testGameId, request.GameId);
    }

    [Fact]
    public void StartGameRequest_空文字列設定時()
    {
        var request = new StartGameRequest { GameId = "" };

        Assert.Equal("", request.GameId);
    }

    [Fact]
    public void StartGameRequest_null設定時()
    {
        var request = new StartGameRequest { GameId = null! };

        Assert.Null(request.GameId);
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("ABCDEF")]
    [InlineData("game_001")]
    [InlineData("ゲーム_001")]
    public void StartGameRequest_様々なGameId値でのテスト(string gameId)
    {
        var request = new StartGameRequest { GameId = gameId };

        Assert.Equal(gameId, request.GameId);
    }
}