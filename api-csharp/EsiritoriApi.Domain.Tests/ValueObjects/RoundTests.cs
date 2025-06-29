namespace EsiritoriApi.Tests.Domain.ValueObjects;

using EsiritoriApi.Domain.ValueObjects;
using EsiritoriApi.Domain.Errors;
using Xunit;

public sealed class RoundTests
{
    [Fact]
    public void 正常な値でRoundが作成される()
    {
        var drawerId = new PlayerId("drawer123");
        var turn = Turn.CreateInitial(drawerId, 60);
        var startTime = DateTime.UtcNow;
        var round = new Round(1, turn, startTime, Option<DateTime>.None());

        Assert.Equal(1, round.RoundNumber);
        Assert.Equal(turn, round.CurrentTurn);
        Assert.Equal(startTime, round.StartedAt);
        Assert.False(round.EndedAt.HasValue);
    }

    [Fact]
    public void ラウンド番号が1未満の場合例外が発生する()
    {
        var drawerId = new PlayerId("drawer123");
        var turn = Turn.CreateInitial(drawerId, 60);
        var exception = Assert.Throws<DomainErrorException>(() => new Round(0, turn, DateTime.UtcNow, Option<DateTime>.None()));
        Assert.Equal(DomainErrorCodes.Round.InvalidRoundNumber, exception.ErrorCode);
    }

    [Fact]
    public void ラウンド番号が10を超える場合例外が発生する()
    {
        var drawerId = new PlayerId("drawer123");
        var turn = Turn.CreateInitial(drawerId, 60);
        var exception = Assert.Throws<DomainErrorException>(() => new Round(11, turn, DateTime.UtcNow, Option<DateTime>.None()));
        Assert.Equal(DomainErrorCodes.Round.InvalidRoundNumber, exception.ErrorCode);
    }

    [Fact]
    public void 現在のターンがnullの場合例外が発生する()
    {
        var exception = Assert.Throws<DomainErrorException>(() => new Round(1, null!, DateTime.UtcNow, Option<DateTime>.None()));
        Assert.Equal(DomainErrorCodes.Round.InvalidCurrentTurn, exception.ErrorCode);
    }

    [Fact]
    public void CreateInitialが正しく動作する()
    {
        var drawerId = new PlayerId("drawer123");
        var turn = Turn.CreateInitial(drawerId, 60);
        var startTime = DateTime.UtcNow;
        var round = Round.CreateInitial(turn, startTime);

        Assert.Equal(1, round.RoundNumber);
        Assert.Equal(turn, round.CurrentTurn);
        Assert.Equal(startTime, round.StartedAt);
        Assert.False(round.EndedAt.HasValue);
    }

    [Fact]
    public void 等価性が正しく判定される()
    {
        var drawerId = new PlayerId("drawer123");
        var turn = Turn.CreateInitial(drawerId, 60);
        var startTime = DateTime.UtcNow;
        var round1 = new Round(1, turn, startTime, Option<DateTime>.None());
        var round2 = new Round(1, turn, startTime, Option<DateTime>.None());
        var round3 = new Round(2, turn, startTime, Option<DateTime>.None());

        Assert.Equal(round1, round2);
        Assert.NotEqual(round1, round3);
    }

    [Fact]
    public void ハッシュコードが正しく計算される()
    {
        var drawerId = new PlayerId("drawer123");
        var turn = Turn.CreateInitial(drawerId, 60);
        var startTime = DateTime.UtcNow;
        var round1 = new Round(1, turn, startTime, Option<DateTime>.None());
        var round2 = new Round(1, turn, startTime, Option<DateTime>.None());

        Assert.Equal(round1.GetHashCode(), round2.GetHashCode());
    }
}
