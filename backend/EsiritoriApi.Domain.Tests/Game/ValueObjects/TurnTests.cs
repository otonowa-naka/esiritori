namespace EsiritoriApi.Tests.Domain.ValueObjects;

using EsiritoriApi.Domain.Game.ValueObjects;
using EsiritoriApi.Domain.Shared.ValueObjects;
using EsiritoriApi.Domain.Errors;
using Xunit;

public sealed class TurnTests
{
    [Fact]
    public void 正常な値でTurnが作成される()
    {
        var drawerId = new PlayerId("drawer123");
        var answer = new Answer("りんご");
        var turn = new Turn(1, drawerId, Option<Answer>.Some(answer), TurnStatus.Drawing, 60, DateTime.UtcNow, Option<DateTime>.None());

        Assert.Equal(1, turn.TurnNumber);
        Assert.Equal(drawerId, turn.DrawerId);
        Assert.Equal(answer, turn.Answer.Value);
        Assert.Equal(TurnStatus.Drawing, turn.Status);
        Assert.Equal(60, turn.TimeLimit);
    }

    [Fact]
    public void ターン番号が1未満の場合例外が発生する()
    {
        var drawerId = new PlayerId("drawer123");
        var exception = Assert.Throws<DomainErrorException>(() => new Turn(0, drawerId, Option<Answer>.None(), TurnStatus.SettingAnswer, 60, DateTime.UtcNow, Option<DateTime>.None()));
        Assert.Equal(DomainErrorCodes.Turn.InvalidTurnNumber, exception.ErrorCode);
    }

    [Fact]
    public void ターン番号が10を超える場合例外が発生する()
    {
        var drawerId = new PlayerId("drawer123");
        var exception = Assert.Throws<DomainErrorException>(() => new Turn(11, drawerId, Option<Answer>.None(), TurnStatus.SettingAnswer, 60, DateTime.UtcNow, Option<DateTime>.None()));
        Assert.Equal(DomainErrorCodes.Turn.InvalidTurnNumber, exception.ErrorCode);
    }

    [Fact]
    public void 制限時間が1秒未満の場合例外が発生する()
    {
        var drawerId = new PlayerId("drawer123");
        var exception = Assert.Throws<DomainErrorException>(() => new Turn(1, drawerId, Option<Answer>.None(), TurnStatus.SettingAnswer, 0, DateTime.UtcNow, Option<DateTime>.None()));
        Assert.Equal(DomainErrorCodes.Turn.InvalidTurnNumber, exception.ErrorCode);
    }

    [Fact]
    public void 制限時間が300秒を超える場合例外が発生する()
    {
        var drawerId = new PlayerId("drawer123");
        var exception = Assert.Throws<DomainErrorException>(() => new Turn(1, drawerId, Option<Answer>.None(), TurnStatus.SettingAnswer, 301, DateTime.UtcNow, Option<DateTime>.None()));
        Assert.Equal(DomainErrorCodes.Turn.InvalidTurnNumber, exception.ErrorCode);
    }

    [Fact]
    public void 描画者IDがnullの場合例外が発生する()
    {
        var exception = Assert.Throws<DomainErrorException>(() => new Turn(1, null!, Option<Answer>.None(), TurnStatus.SettingAnswer, 60, DateTime.UtcNow, Option<DateTime>.None()));
        Assert.Equal(DomainErrorCodes.Turn.InvalidDrawerId, exception.ErrorCode);
    }

    [Fact]
    public void CreateInitialが正しく動作する()
    {
        var drawerId = new PlayerId("drawer123");
        var startedAt = DateTime.UtcNow;
        var turn = Turn.CreateInitial(drawerId, 60, startedAt);

        Assert.Equal(1, turn.TurnNumber);
        Assert.Equal(drawerId, turn.DrawerId);
        Assert.False(turn.Answer.HasValue);
        Assert.Equal(TurnStatus.SettingAnswer, turn.Status);
        Assert.Equal(60, turn.TimeLimit);
        Assert.Equal(startedAt, turn.StartedAt);
        Assert.False(turn.EndedAt.HasValue);
    }

    [Fact]
    public void SetAnswerAndStartDrawingが正しく動作する()
    {
        var drawerId = new PlayerId("drawer123");
        var turn = Turn.CreateInitial(drawerId, 60, DateTime.UtcNow);
        var answer = new Answer("りんご");
        var startTime = DateTime.UtcNow;

        var updatedTurn = turn.SetAnswerAndStartDrawing(answer, startTime);

        Assert.Equal(answer, updatedTurn.Answer.Value);
        Assert.Equal(TurnStatus.Drawing, updatedTurn.Status);
        Assert.Equal(startTime, updatedTurn.StartedAt);
    }

    [Fact]
    public void CheckAnswer_正解の場合正解者リストに追加される()
    {
        var drawerId = new PlayerId("drawer123");
        var answer = new Answer("りんご");
        var turn = new Turn(1, drawerId, Option<Answer>.Some(answer), TurnStatus.Drawing, 60, DateTime.UtcNow, Option<DateTime>.None());
        var playerId = new PlayerId("player123");
        var playerAnswer = new Answer("りんご");
        var answerTime = DateTime.UtcNow;

        var updatedTurn = turn.CheckAnswer(playerAnswer, playerId, answerTime);

        Assert.Equal(TurnStatus.Finished, updatedTurn.Status);
        Assert.Contains(playerId, updatedTurn.CorrectPlayerIds);
        Assert.Equal(answerTime, updatedTurn.EndedAt.Value);
    }

    [Fact]
    public void CheckAnswer_不正解の場合状態が変わらない()
    {
        var drawerId = new PlayerId("drawer123");
        var answer = new Answer("りんご");
        var turn = new Turn(1, drawerId, Option<Answer>.Some(answer), TurnStatus.Drawing, 60, DateTime.UtcNow, Option<DateTime>.None());
        var playerId = new PlayerId("player123");
        var playerAnswer = new Answer("みかん");
        var answerTime = DateTime.UtcNow;

        var updatedTurn = turn.CheckAnswer(playerAnswer, playerId, answerTime);

        Assert.Equal(TurnStatus.Drawing, updatedTurn.Status);
        Assert.Empty(updatedTurn.CorrectPlayerIds);
        Assert.False(updatedTurn.EndedAt.HasValue);
    }

    [Fact]
    public void CheckAnswer_回答がnullの場合例外が発生する()
    {
        var drawerId = new PlayerId("drawer123");
        var answer = new Answer("りんご");
        var turn = new Turn(1, drawerId, Option<Answer>.Some(answer), TurnStatus.Drawing, 60, DateTime.UtcNow, Option<DateTime>.None());
        var playerId = new PlayerId("player123");

        var exception = Assert.Throws<DomainErrorException>(() => turn.CheckAnswer(null!, playerId, DateTime.UtcNow));
        Assert.Equal(DomainErrorCodes.Turn.InvalidTurnNumber, exception.ErrorCode);
    }

    [Fact]
    public void CheckAnswer_プレイヤーIDがnullの場合例外が発生する()
    {
        var drawerId = new PlayerId("drawer123");
        var answer = new Answer("りんご");
        var turn = new Turn(1, drawerId, Option<Answer>.Some(answer), TurnStatus.Drawing, 60, DateTime.UtcNow, Option<DateTime>.None());
        var playerAnswer = new Answer("りんご");

        var exception = Assert.Throws<DomainErrorException>(() => turn.CheckAnswer(playerAnswer, null!, DateTime.UtcNow));
        Assert.Equal(DomainErrorCodes.Turn.InvalidDrawerId, exception.ErrorCode);
    }

    [Fact]
    public void FinishTurnByTimeoutが正しく動作する()
    {
        var drawerId = new PlayerId("drawer123");
        var turn = new Turn(1, drawerId, Option<Answer>.None(), TurnStatus.Drawing, 60, DateTime.UtcNow, Option<DateTime>.None());
        var endTime = DateTime.UtcNow;

        var updatedTurn = turn.FinishTurnByTimeout(endTime);

        Assert.Equal(TurnStatus.Finished, updatedTurn.Status);
        Assert.Equal(endTime, updatedTurn.EndedAt.Value);
    }

    [Fact]
    public void 等価性が正しく判定される()
    {
        var drawerId = new PlayerId("drawer123");
        var answer = new Answer("りんご");
        var startTime = DateTime.UtcNow;
        var turn1 = new Turn(1, drawerId, Option<Answer>.Some(answer), TurnStatus.Drawing, 60, startTime, Option<DateTime>.None());
        var turn2 = new Turn(1, drawerId, Option<Answer>.Some(answer), TurnStatus.Drawing, 60, startTime, Option<DateTime>.None());
        var turn3 = new Turn(2, drawerId, Option<Answer>.Some(answer), TurnStatus.Drawing, 60, startTime, Option<DateTime>.None());

        Assert.Equal(turn1, turn2);
        Assert.NotEqual(turn1, turn3);
    }

    [Fact]
    public void ハッシュコードが正しく計算される()
    {
        var drawerId = new PlayerId("drawer123");
        var answer = new Answer("りんご");
        var startTime = DateTime.UtcNow;
        var turn1 = new Turn(1, drawerId, Option<Answer>.Some(answer), TurnStatus.Drawing, 60, startTime, Option<DateTime>.None());
        var turn2 = new Turn(1, drawerId, Option<Answer>.Some(answer), TurnStatus.Drawing, 60, startTime, Option<DateTime>.None());

        Assert.Equal(turn1.GetHashCode(), turn2.GetHashCode());
    }
}
