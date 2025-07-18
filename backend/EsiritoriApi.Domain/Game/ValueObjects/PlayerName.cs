using EsiritoriApi.Domain.Errors;

namespace EsiritoriApi.Domain.Game.ValueObjects;

public sealed class PlayerName : IEquatable<PlayerName>
{
    public string Value { get; }

    public PlayerName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainErrorException(DomainErrorCodes.Player.InvalidName, "プレイヤー名は空にできません");
        }

        if (value.Trim().Length > 20)
        {
            throw new DomainErrorException(DomainErrorCodes.Player.InvalidName, "プレイヤー名は20文字以下である必要があります");
        }

        Value = value.Trim();
    }

    public bool Equals(PlayerName? other)
    {
        return other is not null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as PlayerName);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static bool operator ==(PlayerName? left, PlayerName? right)
    {
        return EqualityComparer<PlayerName>.Default.Equals(left, right);
    }

    public static bool operator !=(PlayerName? left, PlayerName? right)
    {
        return !(left == right);
    }
}
