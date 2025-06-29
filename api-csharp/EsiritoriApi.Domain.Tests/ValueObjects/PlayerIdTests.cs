namespace EsiritoriApi.Tests.Domain.ValueObjects;

using EsiritoriApi.Domain.Errors;
using EsiritoriApi.Domain.ValueObjects;
using Xunit;

public class PlayerIdTests
{
    [Fact]
    public void 正常なIDでPlayerIdが作成される()
    {
        var id = new PlayerId("player123");

        Assert.Equal("player123", id.Value);
    }

    [Fact]
    public void 空文字の場合例外が発生する()
    {
        var exception = Assert.Throws<DomainErrorException>(() => new PlayerId(""));
        Assert.Equal(DomainErrorCodes.Player.InvalidId, exception.ErrorCode);
    }

    [Fact]
    public void nullの場合例外が発生する()
    {
        var exception = Assert.Throws<DomainErrorException>(() => new PlayerId(null!));
        Assert.Equal(DomainErrorCodes.Player.InvalidId, exception.ErrorCode);
    }

    [Fact]
    public void 空白文字のみの場合例外が発生する()
    {
        var exception = Assert.Throws<DomainErrorException>(() => new PlayerId("   "));
        Assert.Equal(DomainErrorCodes.Player.InvalidId, exception.ErrorCode);
    }

    [Fact]
    public void 前後の空白が除去される()
    {
        var id = new PlayerId("  player123  ");

        Assert.Equal("player123", id.Value);
    }

    [Fact]
    public void 等価性が正しく判定される()
    {
        var id1 = new PlayerId("player123");
        var id2 = new PlayerId("player123");
        var id3 = new PlayerId("player456");

        Assert.Equal(id1, id2);
        Assert.NotEqual(id1, id3);
    }

    [Fact]
    public void ハッシュコードが正しく計算される()
    {
        var id1 = new PlayerId("player123");
        var id2 = new PlayerId("player123");

        Assert.Equal(id1.GetHashCode(), id2.GetHashCode());
    }

    [Fact]
    public void ToStringが正しく動作する()
    {
        var id = new PlayerId("player123");

        Assert.Equal("player123", id.ToString());
    }

    [Fact]
    public void NewIdが正しく動作する()
    {
        var id1 = PlayerId.NewId();
        var id2 = PlayerId.NewId();

        Assert.NotNull(id1.Value);
        Assert.NotNull(id2.Value);
        Assert.NotEqual(id1.Value, id2.Value);
        Assert.True(Guid.TryParse(id1.Value, out _));
        Assert.True(Guid.TryParse(id2.Value, out _));
    }
}
