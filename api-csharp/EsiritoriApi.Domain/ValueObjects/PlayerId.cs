using EsiritoriApi.Domain.Errors;

namespace EsiritoriApi.Domain.ValueObjects;

public sealed class PlayerId : IEquatable<PlayerId>
{
    public string Value { get; }

    public PlayerId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainErrorException(DomainErrorCodes.Player.InvalidId, "プレイヤーIDは空にできません");
        }

        Value = value.Trim();
    }

    public bool Equals(PlayerId? other)
    {
        return other is not null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as PlayerId);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static bool operator ==(PlayerId? left, PlayerId? right)
    {
        return EqualityComparer<PlayerId>.Default.Equals(left, right);
    }

    public static bool operator !=(PlayerId? left, PlayerId? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// 新しいPlayerIdを生成します（GUID）
    /// </summary>
    public static PlayerId NewId()
    {
        return new PlayerId(Guid.NewGuid().ToString());
    }
}
