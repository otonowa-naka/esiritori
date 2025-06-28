using EsiritoriApi.Domain.Errors;

namespace EsiritoriApi.Domain.ValueObjects;

/// <summary>
/// ゲームのラウンドを表す値オブジェクト
/// </summary>
public sealed class Round : IEquatable<Round>
{
    /// <summary>ラウンド番号（1-10）</summary>
    public int RoundNumber { get; private set; }
    /// <summary>現在のターン</summary>
    public Turn CurrentTurn { get; private set; }
    /// <summary>ラウンド開始時刻</summary>
    public DateTime StartedAt { get; private set; }
    /// <summary>ラウンド終了時刻（オプション）</summary>
    public Option<DateTime> EndedAt { get; private set; }

    /// <summary>
    /// ラウンドの新しいインスタンスを作成します
    /// </summary>
    /// <param name="roundNumber">ラウンド番号（1-10）</param>
    /// <param name="currentTurn">現在のターン</param>
    /// <param name="startedAt">開始時刻</param>
    /// <param name="endedAt">終了時刻（オプション）</param>
    /// <exception cref="DomainErrorException">ラウンド番号が1-10の範囲外の場合</exception>
    /// <exception cref="ArgumentNullException">currentTurnがnullの場合</exception>
    public Round(int roundNumber, Turn currentTurn, DateTime startedAt, Option<DateTime> endedAt)
    {
        if (roundNumber < 1 || roundNumber > 10)
        {
            throw new DomainErrorException(DomainErrorCodes.Round.InvalidRoundNumber, "ラウンド番号は1から10の間で設定してください");
        }
        RoundNumber = roundNumber;
        CurrentTurn = currentTurn ?? throw new DomainErrorException(DomainErrorCodes.Round.InvalidCurrentTurn, "現在のターンはnullにできません");
        StartedAt = startedAt;
        EndedAt = endedAt;
    }

    /// <summary>
    /// 初期状態のラウンドを作成します
    /// </summary>
    /// <param name="initialTurn">初期ターン</param>
    /// <param name="startedAt">開始時刻</param>
    /// <returns>初期状態のラウンド</returns>
    public static Round CreateInitial(Turn initialTurn, DateTime startedAt)
    {
        return new Round(1, initialTurn, startedAt, Option<DateTime>.None());
    }

    /// <summary>
    /// 現在のラウンドのコピーを作成します
    /// </summary>
    /// <returns>現在のラウンドのコピー</returns>
    private Round Clone()
    {
        return new Round(RoundNumber, CurrentTurn, StartedAt, EndedAt);
    }

    /// <summary>
    /// 他のラウンドと等価かどうかを判定します
    /// </summary>
    /// <param name="other">比較対象のラウンド</param>
    /// <returns>等価な場合はtrue、そうでない場合はfalse</returns>
    public bool Equals(Round? other)
    {
        return other is not null &&
               RoundNumber == other.RoundNumber &&
               CurrentTurn.Equals(other.CurrentTurn) &&
               StartedAt == other.StartedAt &&
               EndedAt == other.EndedAt;
    }

    /// <summary>
    /// 他のオブジェクトと等価かどうかを判定します
    /// </summary>
    /// <param name="obj">比較対象のオブジェクト</param>
    /// <returns>等価な場合はtrue、そうでない場合はfalse</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as Round);
    }

    /// <summary>
    /// ハッシュコードを取得します
    /// </summary>
    /// <returns>ハッシュコード</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(RoundNumber, CurrentTurn, StartedAt, EndedAt);
    }

    /// <summary>
    /// 等価演算子
    /// </summary>
    /// <param name="left">左辺のラウンド</param>
    /// <param name="right">右辺のラウンド</param>
    /// <returns>等価な場合はtrue、そうでない場合はfalse</returns>
    public static bool operator ==(Round? left, Round? right)
    {
        return EqualityComparer<Round>.Default.Equals(left, right);
    }

    /// <summary>
    /// 不等価演算子
    /// </summary>
    /// <param name="left">左辺のラウンド</param>
    /// <param name="right">右辺のラウンド</param>
    /// <returns>不等価な場合はtrue、そうでない場合はfalse</returns>
    public static bool operator !=(Round? left, Round? right)
    {
        return !(left == right);
    }
}
