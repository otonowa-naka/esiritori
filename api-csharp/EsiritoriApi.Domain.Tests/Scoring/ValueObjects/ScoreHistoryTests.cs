namespace EsiritoriApi.Tests.Domain.ValueObjects;

using EsiritoriApi.Domain.Scoring.ValueObjects;
using EsiritoriApi.Domain.Errors;
using EsiritoriApi.Domain.Game.ValueObjects;
using Xunit;

public sealed class ScoreHistoryTests
{
    [Fact]
    public void 正常な値でScoreHistoryが作成される()
    {
        var playerId = new PlayerId("player123");
        var scoreHistory = new ScoreHistory(playerId, 1, 1, 10, ScoreReason.CorrectAnswer);

        Assert.Equal(playerId, scoreHistory.PlayerId);
        Assert.Equal(1, scoreHistory.RoundNumber);
        Assert.Equal(1, scoreHistory.TurnNumber);
        Assert.Equal(10, scoreHistory.Points);
        Assert.Equal(ScoreReason.CorrectAnswer, scoreHistory.Reason);
        Assert.NotNull(scoreHistory.Timestamp);
    }

    [Fact]
    public void ラウンド番号が1未満の場合例外が発生する()
    {
        var playerId = new PlayerId("player123");
        var exception = Assert.Throws<DomainErrorException>(() => new ScoreHistory(playerId, 0, 1, 10, ScoreReason.CorrectAnswer));
        Assert.Equal(DomainErrorCodes.ScoreHistory.InvalidRoundNumber, exception.ErrorCode);
    }

    [Fact]
    public void ラウンド番号が10を超える場合例外が発生する()
    {
        var playerId = new PlayerId("player123");
        var exception = Assert.Throws<DomainErrorException>(() => new ScoreHistory(playerId, 11, 1, 10, ScoreReason.CorrectAnswer));
        Assert.Equal(DomainErrorCodes.ScoreHistory.InvalidRoundNumber, exception.ErrorCode);
    }

    [Fact]
    public void ターン番号が1未満の場合例外が発生する()
    {
        var playerId = new PlayerId("player123");
        var exception = Assert.Throws<DomainErrorException>(() => new ScoreHistory(playerId, 1, 0, 10, ScoreReason.CorrectAnswer));
        Assert.Equal(DomainErrorCodes.ScoreHistory.InvalidTurnNumber, exception.ErrorCode);
    }

    [Fact]
    public void ターン番号が10を超える場合例外が発生する()
    {
        var playerId = new PlayerId("player123");
        var exception = Assert.Throws<DomainErrorException>(() => new ScoreHistory(playerId, 1, 11, 10, ScoreReason.CorrectAnswer));
        Assert.Equal(DomainErrorCodes.ScoreHistory.InvalidTurnNumber, exception.ErrorCode);
    }

    [Fact]
    public void ポイントが1未満の場合例外が発生する()
    {
        var playerId = new PlayerId("player123");
        var exception = Assert.Throws<DomainErrorException>(() => new ScoreHistory(playerId, 1, 1, 0, ScoreReason.CorrectAnswer));
        Assert.Equal(DomainErrorCodes.ScoreHistory.InvalidPoints, exception.ErrorCode);
    }

    [Fact]
    public void プレイヤーIDがnullの場合例外が発生する()
    {
        var exception = Assert.Throws<DomainErrorException>(() => new ScoreHistory(null!, 1, 1, 10, ScoreReason.CorrectAnswer));
        Assert.Equal(DomainErrorCodes.ScoreHistory.InvalidPlayerId, exception.ErrorCode);
    }

    [Fact]
    public void タイムスタンプが指定されない場合現在時刻が設定される()
    {
        var playerId = new PlayerId("player123");
        var beforeCreation = DateTime.UtcNow;
        var scoreHistory = new ScoreHistory(playerId, 1, 1, 10, ScoreReason.CorrectAnswer);
        var afterCreation = DateTime.UtcNow;

        Assert.True(scoreHistory.Timestamp >= beforeCreation);
        Assert.True(scoreHistory.Timestamp <= afterCreation);
    }

    [Fact]
    public void タイムスタンプが指定された場合その値が設定される()
    {
        var playerId = new PlayerId("player123");
        var timestamp = DateTime.UtcNow.AddHours(-1);
        var scoreHistory = new ScoreHistory(playerId, 1, 1, 10, ScoreReason.CorrectAnswer, timestamp);

        Assert.Equal(timestamp, scoreHistory.Timestamp);
    }

    [Fact]
    public void 等価性が正しく判定される()
    {
        var playerId = new PlayerId("player123");
        var timestamp = DateTime.UtcNow;
        var scoreHistory1 = new ScoreHistory(playerId, 1, 1, 10, ScoreReason.CorrectAnswer, timestamp);
        var scoreHistory2 = new ScoreHistory(playerId, 1, 1, 10, ScoreReason.CorrectAnswer, timestamp);
        var scoreHistory3 = new ScoreHistory(playerId, 2, 1, 10, ScoreReason.CorrectAnswer, timestamp);

        Assert.Equal(scoreHistory1, scoreHistory2);
        Assert.NotEqual(scoreHistory1, scoreHistory3);
    }

    [Fact]
    public void ハッシュコードが正しく計算される()
    {
        var playerId = new PlayerId("player123");
        var timestamp = DateTime.UtcNow;
        var scoreHistory1 = new ScoreHistory(playerId, 1, 1, 10, ScoreReason.CorrectAnswer, timestamp);
        var scoreHistory2 = new ScoreHistory(playerId, 1, 1, 10, ScoreReason.CorrectAnswer, timestamp);

        Assert.Equal(scoreHistory1.GetHashCode(), scoreHistory2.GetHashCode());
    }
}
