using EsiritoriApi.Domain.Errors;

namespace EsiritoriApi.Domain.Game.ValueObjects;

public sealed class GameId : IEquatable<GameId>
{
    public string Value { get; }

    public GameId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainErrorException(DomainErrorCodes.Game.NotFound, "ゲームIDは空にできません");
        }

        Value = value.Trim();
    }

    public bool Equals(GameId? other)
    {
        return other is not null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as GameId);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static bool operator ==(GameId? left, GameId? right)
    {
        return EqualityComparer<GameId>.Default.Equals(left, right);
    }

    public static bool operator !=(GameId? left, GameId? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// 新しいGameIdを生成します（GUID）
    /// </summary>
    public static GameId NewId()
    {
        return new GameId(Guid.NewGuid().ToString());
    }
}
