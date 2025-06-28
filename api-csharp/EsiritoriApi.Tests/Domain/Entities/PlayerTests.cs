namespace EsiritoriApi.Tests.Domain.Entities;

using EsiritoriApi.Domain.Entities;
using EsiritoriApi.Domain.ValueObjects;
using Xunit;

public sealed class PlayerTests
{
    [Fact]
    public void 有効な値でPlayerが正常に作成される()
    {
        var id = new PlayerId("player123");
        var name = new PlayerName("テストプレイヤー");
        var status = PlayerStatus.Ready;
        var isReady = true;
        var isDrawer = false;

        var player = new Player(id, name, status, isReady, isDrawer);

        Assert.Equal(id, player.Id);
        Assert.Equal(name, player.Name);
        Assert.Equal(status, player.Status);
        Assert.Equal(isReady, player.IsReady);
        Assert.Equal(isDrawer, player.IsDrawer);
    }

    [Fact]
    public void CreateInitialファクトリメソッドでPlayerが正常に作成される()
    {
        var name = new PlayerName("テストプレイヤー");

        var player = Player.CreateInitial(name);

        Assert.NotNull(player.Id);
        Assert.Equal(name, player.Name);
        Assert.Equal(PlayerStatus.NotReady, player.Status);
        Assert.False(player.IsReady);
        Assert.False(player.IsDrawer);
    }

    [Fact]
    public void Playerは作成後に不変である()
    {
        var name = new PlayerName("テストプレイヤー");
        var player = Player.CreateInitial(name);

        Assert.NotNull(player.Id);
        Assert.Equal(name, player.Name);
        Assert.Equal(PlayerStatus.NotReady, player.Status);
        Assert.False(player.IsReady);
        Assert.False(player.IsDrawer);
    }

    [Fact]
    public void 同じ値のPlayer同士は等価である()
    {
        var id = new PlayerId("player123");
        var name = new PlayerName("テストプレイヤー");
        var player1 = new Player(id, name, PlayerStatus.Ready, true, false);
        var player2 = new Player(id, name, PlayerStatus.Ready, true, false);

        Assert.Equal(player1, player2);
        Assert.True(player1 == player2);
        Assert.False(player1 != player2);
        Assert.Equal(player1.GetHashCode(), player2.GetHashCode());
    }

    [Fact]
    public void 異なる値のPlayer同士は等価でない()
    {
        var name = new PlayerName("テストプレイヤー");
        var player1 = Player.CreateInitial(name);
        var player2 = Player.CreateInitial(name);

        Assert.NotEqual(player1, player2);
        Assert.False(player1 == player2);
        Assert.True(player1 != player2);
    }

    [Fact]
    public void nullとの比較で等価でない()
    {
        var name = new PlayerName("テストプレイヤー");
        var player = Player.CreateInitial(name);

        Assert.False(player.Equals(null));
        Assert.False(player == null);
        Assert.True(player != null);
    }

    [Fact]
    public void null名前の場合例外が発生する()
    {
        Assert.Throws<ArgumentNullException>(() => Player.CreateInitial(null!));
    }

    [Fact]
    public void CreateInitialで生成されるIDが一意である()
    {
        var name = new PlayerName("テストプレイヤー");
        var player1 = Player.CreateInitial(name);
        var player2 = Player.CreateInitial(name);

        Assert.NotEqual(player1.Id, player2.Id);
    }

    [Fact]
    public void CreateInitialで生成されるIDがGUID形式である()
    {
        var name = new PlayerName("テストプレイヤー");
        var player = Player.CreateInitial(name);

        Assert.Matches(@"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", player.Id.Value);
    }
}
