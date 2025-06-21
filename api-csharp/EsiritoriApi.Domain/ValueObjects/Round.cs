namespace EsiritoriApi.Domain.ValueObjects;

public sealed class Round : IEquatable<Round>
{
    public int RoundNumber { get; }
    public Turn CurrentTurn { get; }
    public DateTime StartedAt { get; }
    public Option<DateTime> EndedAt { get; }

    public Round(int roundNumber, Turn currentTurn, DateTime startedAt, Option<DateTime> endedAt)
    {
        if (roundNumber < 1 || roundNumber > 10)
        {
            throw new ArgumentException("ラウンド番号は1から10の間で設定してください", nameof(roundNumber));
        }

        RoundNumber = roundNumber;
        CurrentTurn = currentTurn ?? throw new ArgumentNullException(nameof(currentTurn));
        StartedAt = startedAt;
        EndedAt = endedAt;
    }

    public static Round CreateNew(Turn initialTurn, DateTime startedAt)
    {
        return new Round(1, initialTurn, startedAt, Option<DateTime>.None());
    }

    public Round WithTurn(Turn turn)
    {
        return new Round(RoundNumber, turn, StartedAt, EndedAt);
    }

    public Round WithStartTime(DateTime startedAt)
    {
        return new Round(RoundNumber, CurrentTurn, startedAt, EndedAt);
    }

    public Round WithEndTime(DateTime endedAt)
    {
        return new Round(RoundNumber, CurrentTurn, StartedAt, Option<DateTime>.Some(endedAt));
    }

    public bool Equals(Round? other)
    {
        return other is not null &&
               RoundNumber == other.RoundNumber &&
               CurrentTurn.Equals(other.CurrentTurn) &&
               StartedAt == other.StartedAt &&
               EndedAt == other.EndedAt;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Round);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(RoundNumber, CurrentTurn, StartedAt, EndedAt);
    }

    public static bool operator ==(Round? left, Round? right)
    {
        return EqualityComparer<Round>.Default.Equals(left, right);
    }

    public static bool operator !=(Round? left, Round? right)
    {
        return !(left == right);
    }
}
