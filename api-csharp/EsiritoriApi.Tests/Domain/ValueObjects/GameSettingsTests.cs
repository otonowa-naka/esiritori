namespace EsiritoriApi.Tests.Domain.ValueObjects;

using EsiritoriApi.Domain.ValueObjects;
using Xunit;

public sealed class GameSettingsTests
{
    [Fact]
    public void 有効な設定値でGameSettingsが正常に作成される()
    {
        var timeLimit = 60;
        var roundCount = 3;
        var playerCount = 4;

        var settings = new GameSettings(timeLimit, roundCount, playerCount);

        Assert.Equal(timeLimit, settings.TimeLimit);
        Assert.Equal(roundCount, settings.RoundCount);
        Assert.Equal(playerCount, settings.PlayerCount);
    }

    [Theory]
    [InlineData(29)] // 最小値未満
    [InlineData(301)] // 最大値超過
    public void 無効な制限時間の場合例外が発生する(int invalidTimeLimit)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new GameSettings(invalidTimeLimit, 3, 4));
        Assert.Equal("制限時間は30秒から300秒の間で設定してください (Parameter 'timeLimit')", exception.Message);
    }

    [Theory]
    [InlineData(0)] // 最小値未満
    [InlineData(11)] // 最大値超過
    public void 無効なラウンド数の場合例外が発生する(int invalidRoundCount)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new GameSettings(60, invalidRoundCount, 4));
        Assert.Equal("ラウンド数は1から10の間で設定してください (Parameter 'roundCount')", exception.Message);
    }

    [Theory]
    [InlineData(1)] // 最小値未満
    [InlineData(9)] // 最大値超過
    public void 無効なプレイヤー数の場合例外が発生する(int invalidPlayerCount)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new GameSettings(60, 3, invalidPlayerCount));
        Assert.Equal("プレイヤー数は2人から8人の間で設定してください (Parameter 'playerCount')", exception.Message);
    }

    [Fact]
    public void 同じ値のGameSettings同士は等価である()
    {
        var settings1 = new GameSettings(60, 3, 4);
        var settings2 = new GameSettings(60, 3, 4);

        Assert.Equal(settings1, settings2);
        Assert.True(settings1 == settings2);
        Assert.False(settings1 != settings2);
        Assert.Equal(settings1.GetHashCode(), settings2.GetHashCode());
    }

    [Fact]
    public void 異なる値のGameSettings同士は等価でない()
    {
        var settings1 = new GameSettings(60, 3, 4);
        var settings2 = new GameSettings(90, 3, 4);

        Assert.NotEqual(settings1, settings2);
        Assert.False(settings1 == settings2);
        Assert.True(settings1 != settings2);
    }

    [Fact]
    public void nullとの比較で等価でない()
    {
        var settings = new GameSettings(60, 3, 4);

        Assert.False(settings.Equals(null));
        Assert.False(settings == null);
        Assert.True(settings != null);
    }
}
