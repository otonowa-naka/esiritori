namespace EsiritoriApi.Domain.ValueObjects;

public enum TurnStatus
{
    NotStarted,
    SettingAnswer,
    Drawing,
    Finished
}

public sealed class Turn : IEquatable<Turn>
{
    public int TurnNumber { get; }
    public PlayerId DrawerId { get; }
    public string Answer { get; }
    public TurnStatus Status { get; }
    public int TimeLimit { get; }
    public DateTime StartedAt { get; }
    public Option<DateTime> EndedAt { get; }
    public IReadOnlyList<PlayerId> CorrectPlayerIds { get; }

    public Turn(int turnNumber, PlayerId drawerId, string answer, TurnStatus status, int timeLimit, 
                DateTime startedAt, Option<DateTime> endedAt, IEnumerable<PlayerId>? correctPlayerIds = null)
    {
        if (turnNumber < 1 || turnNumber > 10)
        {
            throw new ArgumentException("ターン番号は1から10の間で設定してください", nameof(turnNumber));
        }

        if (timeLimit < 1 || timeLimit > 300)
        {
            throw new ArgumentException("制限時間は1秒から300秒の間で設定してください", nameof(timeLimit));
        }

        if (answer.Length > 50)
        {
            throw new ArgumentException("お題は50文字以下である必要があります", nameof(answer));
        }

        if (!string.IsNullOrEmpty(answer) && !System.Text.RegularExpressions.Regex.IsMatch(answer, @"^[\u3041-\u3096]+$"))
        {
            throw new ArgumentException("お題はひらがなで入力してください", nameof(answer));
        }

        TurnNumber = turnNumber;
        DrawerId = drawerId ?? throw new ArgumentNullException(nameof(drawerId));
        Answer = answer;
        Status = status;
        TimeLimit = timeLimit;
        StartedAt = startedAt;
        EndedAt = endedAt;
        CorrectPlayerIds = correctPlayerIds?.ToList().AsReadOnly() ?? new List<PlayerId>().AsReadOnly();
    }

    public static Turn CreateInitial(PlayerId drawerId, int timeLimit)
    {
        return new Turn(1, drawerId, "", TurnStatus.NotStarted, timeLimit,
                       DateTime.MinValue, Option<DateTime>.None());
    }

    public Turn SetAnswerAndStartDrawing(string answer, DateTime startTime)
    {
        return new Turn(TurnNumber, DrawerId, answer, TurnStatus.Drawing, TimeLimit, startTime, EndedAt, CorrectPlayerIds);
    }

    public Turn WithStatus(TurnStatus status)
    {
        return new Turn(TurnNumber, DrawerId, Answer, status, TimeLimit, StartedAt, EndedAt, CorrectPlayerIds);
    }

    public Turn FinishTurn(DateTime endedAt)
    {
        return new Turn(TurnNumber, DrawerId, Answer, TurnStatus.Finished, TimeLimit, StartedAt, Option<DateTime>.Some(endedAt), CorrectPlayerIds);
    }

    public Turn AddCorrectPlayer(PlayerId playerId)
    {
        if (CorrectPlayerIds.Any(id => id.Equals(playerId)))
        {
            return this;
        }

        var updatedCorrectPlayers = CorrectPlayerIds.ToList();
        updatedCorrectPlayers.Add(playerId);
        return new Turn(TurnNumber, DrawerId, Answer, Status, TimeLimit, StartedAt, EndedAt, updatedCorrectPlayers);
    }

    public bool Equals(Turn? other)
    {
        return other is not null &&
               TurnNumber == other.TurnNumber &&
               DrawerId.Equals(other.DrawerId) &&
               Answer == other.Answer &&
               Status == other.Status &&
               TimeLimit == other.TimeLimit &&
               StartedAt == other.StartedAt &&
               EndedAt == other.EndedAt &&
               CorrectPlayerIds.SequenceEqual(other.CorrectPlayerIds);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Turn);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TurnNumber, DrawerId, Answer, Status, TimeLimit, StartedAt, EndedAt, CorrectPlayerIds.Count);
    }

    public static bool operator ==(Turn? left, Turn? right)
    {
        return EqualityComparer<Turn>.Default.Equals(left, right);
    }

    public static bool operator !=(Turn? left, Turn? right)
    {
        return !(left == right);
    }
}
