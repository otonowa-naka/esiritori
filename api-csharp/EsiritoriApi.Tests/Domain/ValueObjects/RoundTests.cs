namespace EsiritoriApi.Tests.Domain.ValueObjects;

using EsiritoriApi.Domain.ValueObjects;
using Xunit;

public sealed class RoundTests
{
    [Fact]
    public void 有効な値でRoundが正常に作成される()
    {
        var roundNumber = 1;
        var drawerId = new PlayerId("drawer123");
        var currentTurn = Turn.CreateInitial(drawerId, 60);
        var startedAt = DateTime.UtcNow;
        var endedAt = Option<DateTime>.None();

        var round = new Round(roundNumber, currentTurn, startedAt, endedAt);

        Assert.Equal(roundNumber, round.RoundNumber);
        Assert.Equal(currentTurn, round.CurrentTurn);
        Assert.Equal(startedAt, round.StartedAt);
        Assert.Equal(endedAt, round.EndedAt);
    }

    [Fact]
    public void ターンを更新できる()
    {
        var roundNumber = 1;
        var drawerId1 = new PlayerId("drawer123");
        var drawerId2 = new PlayerId("drawer456");
        var currentTurn = Turn.CreateInitial(drawerId1, 60);
        var startedAt = DateTime.UtcNow;
        var round = new Round(roundNumber, currentTurn, startedAt, Option<DateTime>.None());

        var nextTurn = new Turn(2, drawerId2, "", TurnStatus.NotStarted, 60, DateTime.MinValue, Option<DateTime>.None());
        var updatedRound = round.WithTurn(nextTurn);

        Assert.Equal(roundNumber, updatedRound.RoundNumber);
        Assert.Equal(nextTurn, updatedRound.CurrentTurn);
        Assert.Equal(2, updatedRound.CurrentTurn.TurnNumber);
    }

    [Fact]
    public void 開始時間を設定できる()
    {
        var drawerId = new PlayerId("drawer123");
        var turn = Turn.CreateInitial(drawerId, 60);
        var originalStartTime = DateTime.UtcNow.AddMinutes(-1);
        var round = new Round(1, turn, originalStartTime, Option<DateTime>.None());
        var newStartTime = DateTime.UtcNow;

        var updatedRound = round.WithStartTime(newStartTime);

        Assert.Equal(newStartTime, updatedRound.StartedAt);
        Assert.Equal(round.RoundNumber, updatedRound.RoundNumber);
        Assert.Equal(round.CurrentTurn, updatedRound.CurrentTurn);
    }

    [Fact]
    public void 終了時間を設定できる()
    {
        var drawerId = new PlayerId("drawer123");
        var turn = Turn.CreateInitial(drawerId, 60);
        var startTime = DateTime.UtcNow;
        var round = new Round(1, turn, startTime, Option<DateTime>.None());
        var endTime = DateTime.UtcNow.AddMinutes(5);

        var updatedRound = round.WithEndTime(endTime);

        Assert.True(updatedRound.EndedAt.HasValue);
        Assert.Equal(endTime, updatedRound.EndedAt.Value);
        Assert.Equal(round.RoundNumber, updatedRound.RoundNumber);
        Assert.Equal(round.CurrentTurn, updatedRound.CurrentTurn);
    }

    [Fact]
    public void 同じ値のRound同士は等価である()
    {
        var drawerId = new PlayerId("drawer123");
        var turn = Turn.CreateInitial(drawerId, 60);
        var startTime = DateTime.UtcNow;
        var endTime = Option<DateTime>.Some(DateTime.UtcNow.AddMinutes(5));
        var round1 = new Round(1, turn, startTime, endTime);
        var round2 = new Round(1, turn, startTime, endTime);

        Assert.Equal(round1, round2);
        Assert.True(round1 == round2);
        Assert.False(round1 != round2);
        Assert.Equal(round1.GetHashCode(), round2.GetHashCode());
    }

    [Fact]
    public void 異なる値のRound同士は等価でない()
    {
        var drawerId = new PlayerId("drawer123");
        var turn1 = Turn.CreateInitial(drawerId, 60);
        var turn2 = new Turn(2, drawerId, "", TurnStatus.NotStarted, 60, DateTime.MinValue, Option<DateTime>.None());
        var startTime = DateTime.UtcNow;
        var round1 = new Round(1, turn1, startTime, Option<DateTime>.None());
        var round2 = new Round(1, turn2, startTime, Option<DateTime>.None());

        Assert.NotEqual(round1, round2);
        Assert.False(round1 == round2);
        Assert.True(round1 != round2);
    }

    [Fact]
    public void nullとの比較で等価でない()
    {
        var drawerId = new PlayerId("drawer123");
        var turn = Turn.CreateInitial(drawerId, 60);
        var round = new Round(1, turn, DateTime.UtcNow, Option<DateTime>.None());

        Assert.False(round.Equals(null));
        Assert.False(round == null);
        Assert.True(round != null);
    }

    [Fact]
    public void 無効なラウンド番号の場合例外が発生する()
    {
        var drawerId = new PlayerId("drawer123");
        var turn = Turn.CreateInitial(drawerId, 60);

        var exception = Assert.Throws<ArgumentException>(() => new Round(0, turn, DateTime.UtcNow, Option<DateTime>.None()));
        Assert.Equal("ラウンド番号は1から10の間で設定してください (Parameter 'roundNumber')", exception.Message);
    }

    [Fact]
    public void nullCurrentTurnの場合例外が発生する()
    {
        Assert.Throws<ArgumentNullException>(() => new Round(1, null!, DateTime.UtcNow, Option<DateTime>.None()));
    }

    [Fact]
    public void CreateNewファクトリメソッドで新しいラウンドが作成される()
    {
        var drawerId = new PlayerId("drawer123");
        var initialTurn = Turn.CreateInitial(drawerId, 60);
        var startTime = DateTime.UtcNow;

        var round = Round.CreateNew(initialTurn, startTime);

        Assert.Equal(1, round.RoundNumber);
        Assert.Equal(initialTurn, round.CurrentTurn);
        Assert.Equal(startTime, round.StartedAt);
        Assert.False(round.EndedAt.HasValue);
    }
}
