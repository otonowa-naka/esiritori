namespace EsiritoriApi.Tests.Domain.ValueObjects;

using EsiritoriApi.Domain.ValueObjects;
using Xunit;

public sealed class TurnTests
{
    [Fact]
    public void 有効な値でTurnが正常に作成される()
    {
        var turnNumber = 1;
        var drawerId = new PlayerId("drawer123");
        var answer = "";
        var status = TurnStatus.NotStarted;
        var timeLimit = 60;
        var startedAt = DateTime.MinValue;
        var endedAt = Option<DateTime>.None();

        var turn = new Turn(turnNumber, drawerId, answer, status, timeLimit, startedAt, endedAt);

        Assert.Equal(turnNumber, turn.TurnNumber);
        Assert.Equal(drawerId, turn.DrawerId);
        Assert.Equal(answer, turn.Answer);
        Assert.Equal(status, turn.Status);
        Assert.Equal(timeLimit, turn.TimeLimit);
        Assert.Equal(startedAt, turn.StartedAt);
        Assert.Equal(endedAt, turn.EndedAt);
    }

    [Fact]
    public void CreateInitialファクトリメソッドでTurnが正常に作成される()
    {
        var drawerId = new PlayerId("drawer123");
        var timeLimit = 60;

        var turn = Turn.CreateInitial(drawerId, timeLimit);

        Assert.Equal(1, turn.TurnNumber);
        Assert.Equal(drawerId, turn.DrawerId);
        Assert.Equal(string.Empty, turn.Answer);
        Assert.Equal(TurnStatus.NotStarted, turn.Status);
        Assert.Equal(timeLimit, turn.TimeLimit);
        Assert.Equal(DateTime.MinValue, turn.StartedAt);
        Assert.False(turn.EndedAt.HasValue);
    }

    [Fact]
    public void ステータスを更新できる()
    {
        var drawerId = new PlayerId("drawer123");
        var turn = Turn.CreateInitial(drawerId, 60);

        var updatedTurn = turn.WithStatus(TurnStatus.Drawing);

        Assert.Equal(TurnStatus.Drawing, updatedTurn.Status);
        Assert.Equal(turn.TurnNumber, updatedTurn.TurnNumber);
        Assert.Equal(turn.DrawerId, updatedTurn.DrawerId);
        Assert.Equal(turn.Answer, updatedTurn.Answer);
        Assert.Equal(turn.TimeLimit, updatedTurn.TimeLimit);
    }

    [Fact]
    public void 回答を設定して描画を開始できる()
    {
        var drawerId = new PlayerId("drawer123");
        var turn = Turn.CreateInitial(drawerId, 60);
        var answer = "ねこ";
        var startTime = DateTime.UtcNow;

        var updatedTurn = turn.SetAnswerAndStartDrawing(answer, startTime);

        Assert.Equal(answer, updatedTurn.Answer);
        Assert.Equal(TurnStatus.Drawing, updatedTurn.Status);
        Assert.Equal(startTime, updatedTurn.StartedAt);
        Assert.Equal(turn.TurnNumber, updatedTurn.TurnNumber);
        Assert.Equal(turn.DrawerId, updatedTurn.DrawerId);
        Assert.Equal(turn.TimeLimit, updatedTurn.TimeLimit);
    }

    [Fact]
    public void 正解プレイヤーを追加できる()
    {
        var drawerId = new PlayerId("drawer123");
        var turn = Turn.CreateInitial(drawerId, 60);
        var correctPlayerId = new PlayerId("correct123");

        var updatedTurn = turn.AddCorrectPlayer(correctPlayerId);

        Assert.Contains(correctPlayerId, updatedTurn.CorrectPlayerIds);
        Assert.Equal(turn.TurnNumber, updatedTurn.TurnNumber);
        Assert.Equal(turn.DrawerId, updatedTurn.DrawerId);
    }

    [Fact]
    public void 同じ値のTurn同士は等価である()
    {
        var drawerId = new PlayerId("drawer123");
        var startTime = DateTime.UtcNow;
        var turn1 = new Turn(1, drawerId, "", TurnStatus.Drawing, 60, startTime, Option<DateTime>.None());
        var turn2 = new Turn(1, drawerId, "", TurnStatus.Drawing, 60, startTime, Option<DateTime>.None());

        Assert.Equal(turn1, turn2);
        Assert.True(turn1 == turn2);
        Assert.False(turn1 != turn2);
        Assert.Equal(turn1.GetHashCode(), turn2.GetHashCode());
    }

    [Fact]
    public void 異なる値のTurn同士は等価でない()
    {
        var drawerId = new PlayerId("drawer123");
        var turn1 = new Turn(1, drawerId, "ねこ", TurnStatus.NotStarted, 60, DateTime.MinValue, Option<DateTime>.None());
        var turn2 = new Turn(1, drawerId, "いぬ", TurnStatus.NotStarted, 60, DateTime.MinValue, Option<DateTime>.None());

        Assert.NotEqual(turn1, turn2);
        Assert.False(turn1 == turn2);
        Assert.True(turn1 != turn2);
    }

    [Fact]
    public void nullとの比較で等価でない()
    {
        var turn = Turn.CreateInitial(new PlayerId("drawer123"), 60);

        Assert.False(turn.Equals(null));
        Assert.False(turn == null);
        Assert.True(turn != null);
    }

    [Fact]
    public void 無効なターン番号の場合例外が発生する()
    {
        var drawerId = new PlayerId("drawer123");

        var exception = Assert.Throws<ArgumentException>(() => new Turn(0, drawerId, "", TurnStatus.NotStarted, 60, DateTime.MinValue, Option<DateTime>.None()));
        Assert.Equal("ターン番号は1から10の間で設定してください (Parameter 'turnNumber')", exception.Message);
    }

    [Fact]
    public void nullDrawerIdの場合例外が発生する()
    {
        Assert.Throws<ArgumentNullException>(() => new Turn(1, null!, "", TurnStatus.NotStarted, 60, DateTime.MinValue, Option<DateTime>.None()));
    }

    [Fact]
    public void 無効な回答の場合例外が発生する()
    {
        var drawerId = new PlayerId("drawer123");

        var exception = Assert.Throws<ArgumentException>(() => new Turn(1, drawerId, "invalid", TurnStatus.NotStarted, 60, DateTime.MinValue, Option<DateTime>.None()));
        Assert.Equal("お題はひらがなで入力してください (Parameter 'answer')", exception.Message);
    }

    [Fact]
    public void ターンを終了できる()
    {
        var drawerId = new PlayerId("drawer123");
        var turn = Turn.CreateInitial(drawerId, 60);
        var endTime = DateTime.UtcNow;

        var updatedTurn = turn.FinishTurn(endTime);

        Assert.Equal(TurnStatus.Finished, updatedTurn.Status);
        Assert.True(updatedTurn.EndedAt.HasValue);
        Assert.Equal(endTime, updatedTurn.EndedAt.Value);
        Assert.Equal(turn.TurnNumber, updatedTurn.TurnNumber);
        Assert.Equal(turn.DrawerId, updatedTurn.DrawerId);
    }
}
