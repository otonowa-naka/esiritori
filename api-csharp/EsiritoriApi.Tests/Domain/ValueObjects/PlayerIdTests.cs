namespace EsiritoriApi.Tests.Domain.ValueObjects;

using EsiritoriApi.Domain.ValueObjects;
using Xunit;

public sealed class PlayerIdTests
{
    [Fact]
    public void 有効なIDでPlayerIdが正常に作成される()
    {
        var id = "player123";

        var playerId = new PlayerId(id);

        Assert.Equal(id, playerId.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void 無効なIDの場合例外が発生する(string invalidId)
    {
        var exception = Assert.Throws<ArgumentException>(() => new PlayerId(invalidId));
        Assert.Equal("プレイヤーIDは空にできません (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void 同じIDのPlayerId同士は等価である()
    {
        var id1 = new PlayerId("player123");
        var id2 = new PlayerId("player123");

        Assert.Equal(id1, id2);
        Assert.True(id1 == id2);
        Assert.False(id1 != id2);
        Assert.Equal(id1.GetHashCode(), id2.GetHashCode());
    }

    [Fact]
    public void 異なるIDのPlayerId同士は等価でない()
    {
        var id1 = new PlayerId("player123");
        var id2 = new PlayerId("player456");

        Assert.NotEqual(id1, id2);
        Assert.False(id1 == id2);
        Assert.True(id1 != id2);
    }

    [Fact]
    public void nullとの比較で等価でない()
    {
        var playerId = new PlayerId("player123");

        Assert.False(playerId.Equals(null));
        Assert.False(playerId == null);
        Assert.True(playerId != null);
    }

    [Fact]
    public void ToStringでIDが返される()
    {
        var id = "player123";
        var playerId = new PlayerId(id);

        Assert.Equal(id, playerId.ToString());
    }

    [Fact]
    public void 前後の空白は自動的にトリムされる()
    {
        var idWithSpaces = "  player123  ";
        var expectedId = "player123";

        var playerId = new PlayerId(idWithSpaces);

        Assert.Equal(expectedId, playerId.Value);
    }
}
