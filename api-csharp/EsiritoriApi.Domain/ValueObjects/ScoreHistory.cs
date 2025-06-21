namespace EsiritoriApi.Domain.ValueObjects;

public enum ScoreReason
{
    CorrectAnswer,
    DrawerPenalty
}

public sealed class ScoreHistory : IEquatable<ScoreHistory>
{
    public PlayerId PlayerId { get; }
    public int RoundNumber { get; }
    public int TurnNumber { get; }
    public int Points { get; }
    public ScoreReason Reason { get; }
    public DateTime Timestamp { get; }

    public ScoreHistory(PlayerId playerId, int roundNumber, int turnNumber, int points, ScoreReason reason, DateTime? timestamp = null)
    {
        if (roundNumber < 1 || roundNumber > 10)
        {
            throw new ArgumentException("ラウンド番号は1から10の間で設定してください", nameof(roundNumber));
        }

        if (turnNumber < 1 || turnNumber > 10)
        {
            throw new ArgumentException("ターン番号は1から10の間で設定してください", nameof(turnNumber));
        }

        if (points < 1)
        {
            throw new ArgumentException("ポイントは1以上の整数である必要があります", nameof(points));
        }

        PlayerId = playerId ?? throw new ArgumentNullException(nameof(playerId));
        RoundNumber = roundNumber;
        TurnNumber = turnNumber;
        Points = points;
        Reason = reason;
        Timestamp = timestamp ?? DateTime.UtcNow;
    }

    public bool Equals(ScoreHistory? other)
    {
        return other is not null &&
               PlayerId.Equals(other.PlayerId) &&
               RoundNumber == other.RoundNumber &&
               TurnNumber == other.TurnNumber &&
               Timestamp == other.Timestamp;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ScoreHistory);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PlayerId, RoundNumber, TurnNumber, Timestamp);
    }

    public static bool operator ==(ScoreHistory? left, ScoreHistory? right)
    {
        return EqualityComparer<ScoreHistory>.Default.Equals(left, right);
    }

    public static bool operator !=(ScoreHistory? left, ScoreHistory? right)
    {
        return !(left == right);
    }
}
