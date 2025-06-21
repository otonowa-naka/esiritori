namespace EsiritoriApi.Tests.Domain.ValueObjects;

using EsiritoriApi.Domain.ValueObjects;
using Xunit;

public sealed class PlayerNameTests
{
    [Fact]
    public void 有効な名前でPlayerNameが正常に作成される()
    {
        var name = "テストプレイヤー";

        var playerName = new PlayerName(name);

        Assert.Equal(name, playerName.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void 無効な名前の場合例外が発生する(string invalidName)
    {
        var exception = Assert.Throws<ArgumentException>(() => new PlayerName(invalidName));
        Assert.Equal("プレイヤー名は空にできません (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void 名前が20文字を超える場合例外が発生する()
    {
        var longName = new string('あ', 21); // 21文字

        var exception = Assert.Throws<ArgumentException>(() => new PlayerName(longName));
        Assert.Equal("プレイヤー名は20文字以下である必要があります (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void 同じ名前のPlayerName同士は等価である()
    {
        var name1 = new PlayerName("テストプレイヤー");
        var name2 = new PlayerName("テストプレイヤー");

        Assert.Equal(name1, name2);
        Assert.True(name1 == name2);
        Assert.False(name1 != name2);
        Assert.Equal(name1.GetHashCode(), name2.GetHashCode());
    }

    [Fact]
    public void 異なる名前のPlayerName同士は等価でない()
    {
        var name1 = new PlayerName("プレイヤー1");
        var name2 = new PlayerName("プレイヤー2");

        Assert.NotEqual(name1, name2);
        Assert.False(name1 == name2);
        Assert.True(name1 != name2);
    }

    [Fact]
    public void 前後の空白は自動的にトリムされる()
    {
        var nameWithSpaces = "  テストプレイヤー  ";
        var expectedName = "テストプレイヤー";

        var playerName = new PlayerName(nameWithSpaces);

        Assert.Equal(expectedName, playerName.Value);
    }
}
