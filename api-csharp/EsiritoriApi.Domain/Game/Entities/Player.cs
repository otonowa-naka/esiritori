namespace EsiritoriApi.Domain.Game.Entities;

using EsiritoriApi.Domain.Game.ValueObjects;

/// <summary>
/// プレイヤーの状態を表す列挙型
/// </summary>
public enum PlayerStatus
{
    /// <summary>準備完了</summary>
    Ready,
    /// <summary>未準備</summary>
    NotReady
}

/// <summary>
/// しりとりゲームのプレイヤーエンティティ
/// </summary>
public sealed class Player : IEquatable<Player>
{
    /// <summary>プレイヤーID</summary>
    public PlayerId Id { get; private set; }
    /// <summary>プレイヤー名</summary>
    public PlayerName Name { get; private set; }
    /// <summary>プレイヤー状態</summary>
    public PlayerStatus Status { get; private set; }
    /// <summary>準備完了フラグ</summary>
    public bool IsReady { get; private set; }
    /// <summary>描画者フラグ</summary>
    public bool IsDrawer { get; private set; }

    /// <summary>
    /// プレイヤーの新しいインスタンスを作成します
    /// </summary>
    public Player(PlayerId id, PlayerName name, PlayerStatus status, bool isReady, bool isDrawer)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Status = status;
        IsReady = isReady;
        IsDrawer = isDrawer;
    }

    /// <summary>
    /// 初期状態のプレイヤーを作成します
    /// </summary>
    public static Player CreateInitial(PlayerName name)
    {
        var id = PlayerId.NewId();
        return new Player(id, name, PlayerStatus.NotReady, false, false);
    }

    #region 等価性・演算子
    /// <summary>
    /// 他のプレイヤーと等価かどうかを判定します
    /// </summary>
    public bool Equals(Player? other)
    {
        return other is not null && Id.Equals(other.Id);
    }

    /// <summary>
    /// 他のオブジェクトと等価かどうかを判定します
    /// </summary>
    public override bool Equals(object? obj)
    {
        return Equals(obj as Player);
    }

    /// <summary>
    /// ハッシュコードを取得します
    /// </summary>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <summary>
    /// 等価演算子
    /// </summary>
    public static bool operator ==(Player? left, Player? right)
    {
        return EqualityComparer<Player>.Default.Equals(left, right);
    }

    /// <summary>
    /// 不等価演算子
    /// </summary>
    public static bool operator !=(Player? left, Player? right)
    {
        return !(left == right);
    }
    #endregion
}
