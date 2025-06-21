namespace EsiritoriApi.Tests.Domain.ValueObjects;

using EsiritoriApi.Domain.ValueObjects;
using Xunit;

public sealed class GameIdTests
{
    [Fact]
    public void 有効なIDでGameIdが正常に作成される()
    {
        var id = "123456";

        var gameId = new GameId(id);

        Assert.Equal(id, gameId.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void 無効なIDの場合例外が発生する(string invalidId)
    {
        var exception = Assert.Throws<ArgumentException>(() => new GameId(invalidId));
        Assert.Equal("ゲームIDは空にできません (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void 同じIDのGameId同士は等価である()
    {
        var id1 = new GameId("123456");
        var id2 = new GameId("123456");

        Assert.Equal(id1, id2);
        Assert.True(id1 == id2);
        Assert.False(id1 != id2);
        Assert.Equal(id1.GetHashCode(), id2.GetHashCode());
    }

    [Fact]
    public void 異なるIDのGameId同士は等価でない()
    {
        var id1 = new GameId("123456");
        var id2 = new GameId("654321");

        Assert.NotEqual(id1, id2);
        Assert.False(id1 == id2);
        Assert.True(id1 != id2);
    }

    [Fact]
    public void nullとの比較で等価でない()
    {
        var gameId = new GameId("123456");

        Assert.False(gameId.Equals(null));
        Assert.False(gameId == null);
        Assert.True(gameId != null);
    }

    [Fact]
    public void ToStringでIDが返される()
    {
        var id = "123456";
        var gameId = new GameId(id);

        Assert.Equal(id, gameId.ToString());
    }

    [Fact]
    public void 前後の空白は自動的にトリムされる()
    {
        var idWithSpaces = "  123456  ";
        var expectedId = "123456";

        var gameId = new GameId(idWithSpaces);

        Assert.Equal(expectedId, gameId.Value);
    }
}
