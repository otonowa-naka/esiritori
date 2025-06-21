namespace EsiritoriApi.Domain.Entities;

using EsiritoriApi.Domain.ValueObjects;

public enum PlayerStatus
{
    Ready,
    NotReady
}

public sealed class Player : IEquatable<Player>
{
    public PlayerId Id { get; }
    public PlayerName Name { get; }
    public PlayerStatus Status { get; }
    public bool IsReady { get; }
    public bool IsDrawer { get; }

    public Player(PlayerId id, PlayerName name, PlayerStatus status, bool isReady, bool isDrawer)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Status = status;
        IsReady = isReady;
        IsDrawer = isDrawer;
    }

    public static Player CreateInitial(PlayerId id, PlayerName name)
    {
        return new Player(id, name, PlayerStatus.NotReady, false, false);
    }

    public bool Equals(Player? other)
    {
        return other is not null && Id.Equals(other.Id);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Player);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Player? left, Player? right)
    {
        return EqualityComparer<Player>.Default.Equals(left, right);
    }

    public static bool operator !=(Player? left, Player? right)
    {
        return !(left == right);
    }
}
