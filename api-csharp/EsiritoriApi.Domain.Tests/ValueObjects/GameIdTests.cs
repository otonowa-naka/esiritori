namespace EsiritoriApi.Tests.Domain.ValueObjects;

using EsiritoriApi.Domain.Errors;
using EsiritoriApi.Domain.ValueObjects;
using Xunit;

public sealed class GameIdTests
{
    [Fact]
    public void 正常なIDでGameIdが作成される()
    {
        var id = new GameId("game123");

        Assert.Equal("game123", id.Value);
    }

    [Fact]
    public void 空文字の場合例外が発生する()
    {
        var exception = Assert.Throws<DomainErrorException>(() => new GameId(""));
        Assert.Equal(DomainErrorCodes.Game.NotFound, exception.ErrorCode);
    }

    [Fact]
    public void nullの場合例外が発生する()
    {
        var exception = Assert.Throws<DomainErrorException>(() => new GameId(null!));
        Assert.Equal(DomainErrorCodes.Game.NotFound, exception.ErrorCode);
    }

    [Fact]
    public void 空白文字のみの場合例外が発生する()
    {
        var exception = Assert.Throws<DomainErrorException>(() => new GameId("   "));
        Assert.Equal(DomainErrorCodes.Game.NotFound, exception.ErrorCode);
    }

    [Fact]
    public void 前後の空白が除去される()
    {
        var id = new GameId("  game123  ");

        Assert.Equal("game123", id.Value);
    }

    [Fact]
    public void 等価性が正しく判定される()
    {
        var id1 = new GameId("game123");
        var id2 = new GameId("game123");
        var id3 = new GameId("game456");

        Assert.Equal(id1, id2);
        Assert.NotEqual(id1, id3);
    }

    [Fact]
    public void ハッシュコードが正しく計算される()
    {
        var id1 = new GameId("game123");
        var id2 = new GameId("game123");

        Assert.Equal(id1.GetHashCode(), id2.GetHashCode());
    }

    [Fact]
    public void ToStringが正しく動作する()
    {
        var id = new GameId("game123");

        Assert.Equal("game123", id.ToString());
    }

    [Fact]
    public void NewIdが正しく動作する()
    {
        var id1 = GameId.NewId();
        var id2 = GameId.NewId();

        Assert.NotNull(id1.Value);
        Assert.NotNull(id2.Value);
        Assert.NotEqual(id1.Value, id2.Value);
        Assert.True(Guid.TryParse(id1.Value, out _));
        Assert.True(Guid.TryParse(id2.Value, out _));
    }
}
