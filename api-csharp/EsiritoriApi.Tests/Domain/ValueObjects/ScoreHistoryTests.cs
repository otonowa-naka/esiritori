namespace EsiritoriApi.Tests.Domain.ValueObjects;

using EsiritoriApi.Domain.ValueObjects;
using Xunit;

public sealed class ScoreHistoryTests
{
    [Fact]
    public void 有効な値でScoreHistoryが正常に作成される()
    {
        var playerId = new PlayerId("player123");
        var roundNumber = 1;
        var turnNumber = 1;
        var points = 100;
        var reason = ScoreReason.CorrectAnswer;
        var timestamp = DateTime.UtcNow;

        var scoreHistory = new ScoreHistory(playerId, roundNumber, turnNumber, points, reason, timestamp);

        Assert.Equal(playerId, scoreHistory.PlayerId);
        Assert.Equal(roundNumber, scoreHistory.RoundNumber);
        Assert.Equal(turnNumber, scoreHistory.TurnNumber);
        Assert.Equal(points, scoreHistory.Points);
        Assert.Equal(reason, scoreHistory.Reason);
        Assert.Equal(timestamp, scoreHistory.Timestamp);
    }

    [Fact]
    public void 正解による得点記録が正常に作成される()
    {
        var playerId = new PlayerId("player123");
        var scoreHistory = new ScoreHistory(playerId, 1, 1, 100, ScoreReason.CorrectAnswer, DateTime.UtcNow);

        Assert.Equal(ScoreReason.CorrectAnswer, scoreHistory.Reason);
        Assert.Equal(100, scoreHistory.Points);
    }

    [Fact]
    public void 描画者ペナルティによる得点記録が正常に作成される()
    {
        var playerId = new PlayerId("player123");
        var scoreHistory = new ScoreHistory(playerId, 1, 1, 50, ScoreReason.DrawerPenalty, DateTime.UtcNow);

        Assert.Equal(ScoreReason.DrawerPenalty, scoreHistory.Reason);
        Assert.Equal(50, scoreHistory.Points);
    }

    [Fact]
    public void 同じ値のScoreHistory同士は等価である()
    {
        var playerId = new PlayerId("player123");
        var timestamp = DateTime.UtcNow;
        var score1 = new ScoreHistory(playerId, 1, 1, 100, ScoreReason.CorrectAnswer, timestamp);
        var score2 = new ScoreHistory(playerId, 1, 1, 100, ScoreReason.CorrectAnswer, timestamp);

        Assert.Equal(score1, score2);
        Assert.True(score1 == score2);
        Assert.False(score1 != score2);
        Assert.Equal(score1.GetHashCode(), score2.GetHashCode());
    }

    [Fact]
    public void 異なる値のScoreHistory同士は等価でない()
    {
        var playerId1 = new PlayerId("player123");
        var playerId2 = new PlayerId("player456");
        var timestamp = DateTime.UtcNow;
        var score1 = new ScoreHistory(playerId1, 1, 1, 100, ScoreReason.CorrectAnswer, timestamp);
        var score2 = new ScoreHistory(playerId2, 1, 1, 100, ScoreReason.CorrectAnswer, timestamp);

        Assert.NotEqual(score1, score2);
        Assert.False(score1 == score2);
        Assert.True(score1 != score2);
    }

    [Fact]
    public void nullとの比較で等価でない()
    {
        var playerId = new PlayerId("player123");
        var scoreHistory = new ScoreHistory(playerId, 1, 1, 100, ScoreReason.CorrectAnswer, DateTime.UtcNow);

        Assert.False(scoreHistory.Equals(null));
        Assert.False(scoreHistory == null);
        Assert.True(scoreHistory != null);
    }



    [Fact]
    public void nullプレイヤーIDの場合例外が発生する()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ScoreHistory(null!, 1, 1, 100, ScoreReason.CorrectAnswer, DateTime.UtcNow));
    }

    [Fact]
    public void 無効なラウンド数の場合例外が発生する()
    {
        var playerId = new PlayerId("player123");

        var exception = Assert.Throws<ArgumentException>(() =>
            new ScoreHistory(playerId, 0, 1, 100, ScoreReason.CorrectAnswer, DateTime.UtcNow));
        Assert.Equal("ラウンド番号は1から10の間で設定してください (Parameter 'roundNumber')", exception.Message);
    }

    [Fact]
    public void 無効なターン数の場合例外が発生する()
    {
        var playerId = new PlayerId("player123");

        var exception = Assert.Throws<ArgumentException>(() =>
            new ScoreHistory(playerId, 1, 0, 100, ScoreReason.CorrectAnswer, DateTime.UtcNow));
        Assert.Equal("ターン番号は1から10の間で設定してください (Parameter 'turnNumber')", exception.Message);
    }

    [Fact]
    public void 無効なポイントの場合例外が発生する()
    {
        var playerId = new PlayerId("player123");

        var exception = Assert.Throws<ArgumentException>(() =>
            new ScoreHistory(playerId, 1, 1, 0, ScoreReason.CorrectAnswer, DateTime.UtcNow));
        Assert.Equal("ポイントは1以上の整数である必要があります (Parameter 'points')", exception.Message);
    }
}
