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
        var answer = Answer.Empty();
        var status = TurnStatus.SettingAnswer;
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
        Assert.Equal(Answer.Empty(), turn.Answer);
        Assert.Equal(TurnStatus.SettingAnswer, turn.Status);
        Assert.Equal(timeLimit, turn.TimeLimit);
        Assert.Equal(DateTime.MinValue, turn.StartedAt);
        Assert.False(turn.EndedAt.HasValue);
    }

    [Fact]
    public void お題を設定して描画を開始できる()
    {
        var drawerId = new PlayerId("drawer123");
        var turn = Turn.CreateInitial(drawerId, 60);
        var answer = new Answer("ねこ");
        var startTime = DateTime.UtcNow;

        var updatedTurn = turn.SetAnswerAndStartDrawing(answer, startTime);

        Assert.Equal(answer, updatedTurn.Answer);
        Assert.Equal(TurnStatus.Drawing, updatedTurn.Status);
        Assert.Equal(startTime, updatedTurn.StartedAt);
        Assert.Equal(turn.TurnNumber, updatedTurn.TurnNumber);
        Assert.Equal(turn.DrawerId, updatedTurn.DrawerId);
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
    public void 正解の回答でターンが終了する()
    {
        var drawerId = new PlayerId("drawer123");
        var turn = Turn.CreateInitial(drawerId, 60);
        var answer = new Answer("ねこ");
        var startTime = DateTime.UtcNow;
        var drawingTurn = turn.SetAnswerAndStartDrawing(answer, startTime);
        var playerAnswer = new Answer("ねこ");
        var playerId = new PlayerId("player123");
        var answerTime = DateTime.UtcNow;

        var updatedTurn = drawingTurn.CheckAnswer(playerAnswer, playerId, answerTime);

        Assert.Equal(TurnStatus.Finished, updatedTurn.Status);
        Assert.True(updatedTurn.EndedAt.HasValue);
        Assert.Equal(answerTime, updatedTurn.EndedAt.Value);
        Assert.Contains(playerId, updatedTurn.CorrectPlayerIds);
    }

    [Fact]
    public void 不正解の回答でターンが継続する()
    {
        var drawerId = new PlayerId("drawer123");
        var turn = Turn.CreateInitial(drawerId, 60);
        var answer = new Answer("ねこ");
        var startTime = DateTime.UtcNow;
        var drawingTurn = turn.SetAnswerAndStartDrawing(answer, startTime);
        var playerAnswer = new Answer("いぬ");
        var playerId = new PlayerId("player123");
        var answerTime = DateTime.UtcNow;

        var updatedTurn = drawingTurn.CheckAnswer(playerAnswer, playerId, answerTime);

        Assert.Equal(TurnStatus.Drawing, updatedTurn.Status);
        Assert.False(updatedTurn.EndedAt.HasValue);
        Assert.DoesNotContain(playerId, updatedTurn.CorrectPlayerIds);
    }

    [Fact]
    public void 空白を含む正解の回答で正解と判定される()
    {
        var drawerId = new PlayerId("drawer123");
        var turn = Turn.CreateInitial(drawerId, 60);
        var answer = new Answer("ねこ");
        var startTime = DateTime.UtcNow;
        var drawingTurn = turn.SetAnswerAndStartDrawing(answer, startTime);
        
        var playerId = new PlayerId("player123");
        var playerAnswer = new Answer(" ねこ ");
        var answerTime = DateTime.UtcNow;

        var updatedTurn = drawingTurn.CheckAnswer(playerAnswer, playerId, answerTime);

        Assert.Equal(TurnStatus.Finished, updatedTurn.Status);
        Assert.Contains(playerId, updatedTurn.CorrectPlayerIds);
    }

    [Fact]
    public void 同じ値のTurn同士は等価である()
    {
        var drawerId = new PlayerId("drawer123");
        var startTime = DateTime.UtcNow;
        var turn1 = new Turn(1, drawerId, Answer.Empty(), TurnStatus.Drawing, 60, startTime, Option<DateTime>.None());
        var turn2 = new Turn(1, drawerId, Answer.Empty(), TurnStatus.Drawing, 60, startTime, Option<DateTime>.None());

        Assert.Equal(turn1, turn2);
        Assert.True(turn1 == turn2);
        Assert.False(turn1 != turn2);
        Assert.Equal(turn1.GetHashCode(), turn2.GetHashCode());
    }

    [Fact]
    public void 異なる値のTurn同士は等価でない()
    {
        var drawerId = new PlayerId("drawer123");
        var turn1 = new Turn(1, drawerId, new Answer("ねこ"), TurnStatus.SettingAnswer, 60, DateTime.MinValue, Option<DateTime>.None());
        var turn2 = new Turn(1, drawerId, new Answer("いぬ"), TurnStatus.SettingAnswer, 60, DateTime.MinValue, Option<DateTime>.None());

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

        var exception = Assert.Throws<ArgumentException>(() => new Turn(0, drawerId, Answer.Empty(), TurnStatus.SettingAnswer, 60, DateTime.MinValue, Option<DateTime>.None()));
        Assert.Equal("ターン番号は1から10の間で設定してください (Parameter 'turnNumber')", exception.Message);
    }

    [Fact]
    public void nullDrawerIdの場合例外が発生する()
    {
        Assert.Throws<ArgumentNullException>(() => new Turn(1, null!, Answer.Empty(), TurnStatus.SettingAnswer, 60, DateTime.MinValue, Option<DateTime>.None()));
    }

    [Fact]
    public void 時間切れでターンを終了できる()
    {
        var drawerId = new PlayerId("drawer123");
        var turn = Turn.CreateInitial(drawerId, 60);
        var endTime = DateTime.UtcNow;

        var updatedTurn = turn.FinishTurnByTimeout(endTime);

        Assert.Equal(TurnStatus.Finished, updatedTurn.Status);
        Assert.True(updatedTurn.EndedAt.HasValue);
        Assert.Equal(endTime, updatedTurn.EndedAt.Value);
        Assert.Equal(turn.TurnNumber, updatedTurn.TurnNumber);
        Assert.Equal(turn.DrawerId, updatedTurn.DrawerId);
    }

    [Fact]
    public void 空の回答でCheckAnswerが不正解として継続する()
    {
        var drawerId = new PlayerId("drawer123");
        var turn = Turn.CreateInitial(drawerId, 60);
        var answer = new Answer("ねこ");
        var startTime = DateTime.UtcNow;
        var drawingTurn = turn.SetAnswerAndStartDrawing(answer, startTime);
        var playerId = new PlayerId("player123");
        var emptyAnswer = Answer.Empty();
        var answerTime = DateTime.UtcNow;

        var updatedTurn = drawingTurn.CheckAnswer(emptyAnswer, playerId, answerTime);

        // 空の回答は不正解として扱われ、ターンは継続状態のまま
        Assert.Equal(TurnStatus.Drawing, updatedTurn.Status);
        Assert.False(updatedTurn.EndedAt.HasValue);
        Assert.Empty(updatedTurn.CorrectPlayerIds);
    }

    [Fact]
    public void nullのプレイヤーIDでCheckAnswerが例外を発生する()
    {
        var drawerId = new PlayerId("drawer123");
        var turn = Turn.CreateInitial(drawerId, 60);
        var answer = new Answer("ねこ");
        var startTime = DateTime.UtcNow;
        var drawingTurn = turn.SetAnswerAndStartDrawing(answer, startTime);
        
        PlayerId? playerId = null;
        var playerAnswer = new Answer("ねこ");
        var answerTime = DateTime.UtcNow;

        Assert.Throws<ArgumentNullException>(() => drawingTurn.CheckAnswer(playerAnswer, playerId!, answerTime));
    }
}
