namespace EsiritoriApi.Domain.ValueObjects;

/// <summary>
/// ターンの状態を表す列挙型
/// </summary>
public enum TurnStatus
{
    /// <summary>回答設定中</summary>
    SettingAnswer,
    /// <summary>描画中</summary>
    Drawing,
    /// <summary>終了（正解者がいるか、時間切れ）</summary>
    Finished
}

/// <summary>
/// ゲームのターンを表す値オブジェクト
/// 
/// ターンは以下の状態遷移を行います：
/// SettingAnswer → Drawing → Finished
/// 
/// 各ターンでは：
/// - 1人のプレイヤーが描画者として選ばれる
/// - 描画者がお題を設定する
/// - 描画者が制限時間内にお題を描画する
/// - 他のプレイヤーがお題を当てる
/// - 正解者が出るか時間切れでターン終了
/// </summary>
public sealed class Turn : IEquatable<Turn>
{
    /// <summary>ターン番号（1-10）</summary>
    public int TurnNumber { get; private set; }
    
    /// <summary>描画者のプレイヤーID</summary>
    public PlayerId DrawerId { get; private set; }
    
    /// <summary>お題（ひらがな、1-50文字）</summary>
    public Answer Answer { get; private set; }
    
    /// <summary>ターンの現在の状態</summary>
    public TurnStatus Status { get; private set; }
    
    /// <summary>制限時間（秒、1-300）</summary>
    public int TimeLimit { get; private set; }
    
    /// <summary>ターン開始時刻</summary>
    public DateTime StartedAt { get; private set; }
    
    /// <summary>ターン終了時刻（オプション）</summary>
    public Option<DateTime> EndedAt { get; private set; }
    
    /// <summary>正解者のプレイヤーIDリスト</summary>
    public IReadOnlyList<PlayerId> CorrectPlayerIds { get; private set; }

    /// <summary>
    /// ターンの新しいインスタンスを作成します
    /// </summary>
    /// <param name="turnNumber">ターン番号（1-10）</param>
    /// <param name="drawerId">描画者のプレイヤーID</param>
    /// <param name="answer">お題</param>
    /// <param name="status">ターンの状態</param>
    /// <param name="timeLimit">制限時間（秒、1-300）</param>
    /// <param name="startedAt">開始時刻</param>
    /// <param name="endedAt">終了時刻（オプション）</param>
    /// <param name="correctPlayerIds">正解者のプレイヤーIDリスト（オプション）</param>
    /// <exception cref="ArgumentException">
    /// ターン番号が1-10の範囲外、制限時間が1-300の範囲外の場合
    /// </exception>
    /// <exception cref="ArgumentNullException">drawerIdがnullの場合</exception>
    public Turn(int turnNumber, PlayerId drawerId, Answer answer, TurnStatus status, int timeLimit, 
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

        TurnNumber = turnNumber;
        DrawerId = drawerId ?? throw new ArgumentNullException(nameof(drawerId));
        Answer = answer ?? throw new ArgumentNullException(nameof(answer));
        Status = status;
        TimeLimit = timeLimit;
        StartedAt = startedAt;
        EndedAt = endedAt;
        CorrectPlayerIds = correctPlayerIds?.ToList().AsReadOnly() ?? new List<PlayerId>().AsReadOnly();
    }

    /// <summary>
    /// 初期状態のターンを作成します
    /// </summary>
    /// <param name="drawerId">描画者のプレイヤーID</param>
    /// <param name="timeLimit">制限時間（秒）</param>
    /// <returns>初期状態のターン</returns>
    public static Turn CreateInitial(PlayerId drawerId, int timeLimit)
    {
        return new Turn(1, drawerId, Answer.Empty(), TurnStatus.SettingAnswer, timeLimit,
                       DateTime.MinValue, Option<DateTime>.None());
    }

    /// <summary>
    /// 現在のターンのコピーを作成します
    /// </summary>
    /// <returns>現在のターンのコピー</returns>
    private Turn Clone()
    {
        return new Turn(TurnNumber, DrawerId, Answer, Status, TimeLimit, StartedAt, EndedAt, CorrectPlayerIds);
    }

    /// <summary>
    /// お題を設定して描画を開始します
    /// </summary>
    /// <param name="answer">設定するお題</param>
    /// <param name="startTime">描画開始時刻</param>
    /// <returns>お題が設定され描画状態になった新しいターン</returns>
    public Turn SetAnswerAndStartDrawing(Answer answer, DateTime startTime)
    {
        var clone = Clone();
        clone.Answer = answer;
        clone.Status = TurnStatus.Drawing;
        clone.StartedAt = startTime;
        return clone;
    }

    /// <summary>
    /// 回答者の答えをチェックしてターンの状態を更新します
    /// </summary>
    /// <param name="playerAnswer">回答者の答え</param>
    /// <param name="playerId">回答者のプレイヤーID</param>
    /// <param name="endedAt">回答時刻</param>
    /// <returns>正解の場合は終了状態、間違いの場合は継続状態の新しいターン</returns>
    public Turn CheckAnswer(Answer playerAnswer, PlayerId playerId, DateTime endedAt)
    {
        if (playerAnswer == null)
        {
            throw new ArgumentNullException(nameof(playerAnswer));
        }

        if (playerId == null)
        {
            throw new ArgumentNullException(nameof(playerId));
        }

        // 回答が正解かどうかをチェック
        if (Answer.IsCorrect(playerAnswer))
        {
            // 正解の場合：正解者を追加して終了
            var clone = AddCorrectPlayer(playerId);
            clone.Status = TurnStatus.Finished;
            clone.EndedAt = Option<DateTime>.Some(endedAt);
            return clone;
        }
        else
        {
            // 不正解の場合：継続（現在の状態を維持）
            return this;
        }
    }

    /// <summary>
    /// 時間切れでターンを終了します
    /// </summary>
    /// <param name="endedAt">終了時刻</param>
    /// <returns>終了状態になった新しいターン</returns>
    public Turn FinishTurnByTimeout(DateTime endedAt)
    {
        var clone = Clone();
        clone.Status = TurnStatus.Finished;
        clone.EndedAt = Option<DateTime>.Some(endedAt);
        return clone;
    }

    /// <summary>
    /// 正解者を追加します
    /// </summary>
    /// <param name="playerId">正解者のプレイヤーID</param>
    /// <returns>正解者が追加された新しいターン（既に正解者リストに含まれている場合は変更なし）</returns>
    public Turn AddCorrectPlayer(PlayerId playerId)
    {
        if (CorrectPlayerIds.Any(id => id.Equals(playerId)))
        {
            return this;
        }

        var updatedCorrectPlayers = CorrectPlayerIds.ToList();
        updatedCorrectPlayers.Add(playerId);
        
        var clone = Clone();
        clone.CorrectPlayerIds = updatedCorrectPlayers.AsReadOnly();
        return clone;
    }

    /// <summary>
    /// 他のターンと等価かどうかを判定します
    /// </summary>
    /// <param name="other">比較対象のターン</param>
    /// <returns>等価な場合はtrue、そうでない場合はfalse</returns>
    public bool Equals(Turn? other)
    {
        return other is not null &&
               TurnNumber == other.TurnNumber &&
               DrawerId.Equals(other.DrawerId) &&
               Answer.Equals(other.Answer) &&
               Status == other.Status &&
               TimeLimit == other.TimeLimit &&
               StartedAt == other.StartedAt &&
               EndedAt == other.EndedAt &&
               CorrectPlayerIds.SequenceEqual(other.CorrectPlayerIds);
    }

    /// <summary>
    /// 他のオブジェクトと等価かどうかを判定します
    /// </summary>
    /// <param name="obj">比較対象のオブジェクト</param>
    /// <returns>等価な場合はtrue、そうでない場合はfalse</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as Turn);
    }

    /// <summary>
    /// ハッシュコードを取得します
    /// </summary>
    /// <returns>ハッシュコード</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(TurnNumber, DrawerId, Answer, Status, TimeLimit, StartedAt, EndedAt, CorrectPlayerIds.Count);
    }

    /// <summary>
    /// 等価演算子
    /// </summary>
    /// <param name="left">左辺のターン</param>
    /// <param name="right">右辺のターン</param>
    /// <returns>等価な場合はtrue、そうでない場合はfalse</returns>
    public static bool operator ==(Turn? left, Turn? right)
    {
        return EqualityComparer<Turn>.Default.Equals(left, right);
    }

    /// <summary>
    /// 不等価演算子
    /// </summary>
    /// <param name="left">左辺のターン</param>
    /// <param name="right">右辺のターン</param>
    /// <returns>不等価な場合はtrue、そうでない場合はfalse</returns>
    public static bool operator !=(Turn? left, Turn? right)
    {
        return !(left == right);
    }
}
