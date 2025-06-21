namespace EsiritoriApi.Domain.ValueObjects;

public sealed class GameSettings : IEquatable<GameSettings>
{
    public int TimeLimit { get; }
    public int RoundCount { get; }
    public int PlayerCount { get; }

    public GameSettings(int timeLimit, int roundCount, int playerCount)
    {
        if (timeLimit < 30 || timeLimit > 300)
        {
            throw new ArgumentException("制限時間は30秒から300秒の間で設定してください", nameof(timeLimit));
        }

        if (roundCount < 1 || roundCount > 10)
        {
            throw new ArgumentException("ラウンド数は1から10の間で設定してください", nameof(roundCount));
        }

        if (playerCount < 2 || playerCount > 8)
        {
            throw new ArgumentException("プレイヤー数は2人から8人の間で設定してください", nameof(playerCount));
        }

        TimeLimit = timeLimit;
        RoundCount = roundCount;
        PlayerCount = playerCount;
    }

    public bool Equals(GameSettings? other)
    {
        return other is not null &&
               TimeLimit == other.TimeLimit &&
               RoundCount == other.RoundCount &&
               PlayerCount == other.PlayerCount;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as GameSettings);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TimeLimit, RoundCount, PlayerCount);
    }

    public static bool operator ==(GameSettings? left, GameSettings? right)
    {
        return EqualityComparer<GameSettings>.Default.Equals(left, right);
    }

    public static bool operator !=(GameSettings? left, GameSettings? right)
    {
        return !(left == right);
    }
}
