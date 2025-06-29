namespace EsiritoriApi.Tests.Domain.ValueObjects;

using EsiritoriApi.Domain.Errors;
using EsiritoriApi.Domain.Game.ValueObjects;
using Xunit;

public class PlayerNameTests
{
    [Fact]
    public void 正常な名前でPlayerNameが作成される()
    {
        var name = new PlayerName("テストプレイヤー");

        Assert.Equal("テストプレイヤー", name.Value);
    }

    [Fact]
    public void 空文字の場合例外が発生する()
    {
        var exception = Assert.Throws<DomainErrorException>(() => new PlayerName(""));
        Assert.Equal(DomainErrorCodes.Player.InvalidName, exception.ErrorCode);
    }

    [Fact]
    public void nullの場合例外が発生する()
    {
        var exception = Assert.Throws<DomainErrorException>(() => new PlayerName(null!));
        Assert.Equal(DomainErrorCodes.Player.InvalidName, exception.ErrorCode);
    }

    [Fact]
    public void 空白文字のみの場合例外が発生する()
    {
        var exception = Assert.Throws<DomainErrorException>(() => new PlayerName("   "));
        Assert.Equal(DomainErrorCodes.Player.InvalidName, exception.ErrorCode);
    }

    [Fact]
    public void _20文字を超える場合例外が発生する()
    {
        var longName = new string('あ', 21);
        var exception = Assert.Throws<DomainErrorException>(() => new PlayerName(longName));
        Assert.Equal(DomainErrorCodes.Player.InvalidName, exception.ErrorCode);
    }

    [Fact]
    public void _20文字の場合は正常に作成される()
    {
        var name = new PlayerName(new string('あ', 20));

        Assert.Equal(new string('あ', 20), name.Value);
    }

    [Fact]
    public void 前後の空白が除去される()
    {
        var name = new PlayerName("  テストプレイヤー  ");

        Assert.Equal("テストプレイヤー", name.Value);
    }

    [Fact]
    public void 等価性が正しく判定される()
    {
        var name1 = new PlayerName("テストプレイヤー");
        var name2 = new PlayerName("テストプレイヤー");
        var name3 = new PlayerName("別のプレイヤー");

        Assert.Equal(name1, name2);
        Assert.NotEqual(name1, name3);
    }

    [Fact]
    public void ハッシュコードが正しく計算される()
    {
        var name1 = new PlayerName("テストプレイヤー");
        var name2 = new PlayerName("テストプレイヤー");

        Assert.Equal(name1.GetHashCode(), name2.GetHashCode());
    }

    [Fact]
    public void ToStringが正しく動作する()
    {
        var name = new PlayerName("テストプレイヤー");

        Assert.Equal("テストプレイヤー", name.ToString());
    }
}
