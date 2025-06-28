namespace EsiritoriApi.Tests.Domain.ValueObjects;

using EsiritoriApi.Domain.Errors;
using EsiritoriApi.Domain.ValueObjects;
using Xunit;

public sealed class GameSettingsTests
{
    [Fact]
    public void 正常な値でGameSettingsが作成される()
    {
        var settings = new GameSettings(60, 3, 4);

        Assert.Equal(60, settings.TimeLimit);
        Assert.Equal(3, settings.RoundCount);
        Assert.Equal(4, settings.PlayerCount);
    }

    [Fact]
    public void 制限時間が30秒未満の場合例外が発生する()
    {
        var exception = Assert.Throws<DomainErrorException>(() => new GameSettings(29, 3, 4));
        Assert.Equal(DomainErrorCodes.GameSettings.InvalidTimeLimit, exception.ErrorCode);
    }

    [Fact]
    public void 制限時間が300秒を超える場合例外が発生する()
    {
        var exception = Assert.Throws<DomainErrorException>(() => new GameSettings(301, 3, 4));
        Assert.Equal(DomainErrorCodes.GameSettings.InvalidTimeLimit, exception.ErrorCode);
    }

    [Fact]
    public void ラウンド数が1未満の場合例外が発生する()
    {
        var exception = Assert.Throws<DomainErrorException>(() => new GameSettings(60, 0, 4));
        Assert.Equal(DomainErrorCodes.GameSettings.InvalidRoundCount, exception.ErrorCode);
    }

    [Fact]
    public void ラウンド数が10を超える場合例外が発生する()
    {
        var exception = Assert.Throws<DomainErrorException>(() => new GameSettings(60, 11, 4));
        Assert.Equal(DomainErrorCodes.GameSettings.InvalidRoundCount, exception.ErrorCode);
    }

    [Fact]
    public void プレイヤー数が2人未満の場合例外が発生する()
    {
        var exception = Assert.Throws<DomainErrorException>(() => new GameSettings(60, 3, 1));
        Assert.Equal(DomainErrorCodes.GameSettings.InvalidPlayerCount, exception.ErrorCode);
    }

    [Fact]
    public void プレイヤー数が8人を超える場合例外が発生する()
    {
        var exception = Assert.Throws<DomainErrorException>(() => new GameSettings(60, 3, 9));
        Assert.Equal(DomainErrorCodes.GameSettings.InvalidPlayerCount, exception.ErrorCode);
    }

    [Fact]
    public void 等価性が正しく判定される()
    {
        var settings1 = new GameSettings(60, 3, 4);
        var settings2 = new GameSettings(60, 3, 4);
        var settings3 = new GameSettings(90, 5, 6);

        Assert.Equal(settings1, settings2);
        Assert.NotEqual(settings1, settings3);
    }

    [Fact]
    public void ハッシュコードが正しく計算される()
    {
        var settings1 = new GameSettings(60, 3, 4);
        var settings2 = new GameSettings(60, 3, 4);

        Assert.Equal(settings1.GetHashCode(), settings2.GetHashCode());
    }
}
