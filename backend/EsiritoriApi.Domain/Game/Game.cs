namespace EsiritoriApi.Domain.Game;

using EsiritoriApi.Domain.Game.ValueObjects;
using EsiritoriApi.Domain.Game.Entities;
using EsiritoriApi.Domain.Errors;
using EsiritoriApi.Domain.Scoring.ValueObjects;

/// <summary>
/// ゲームの状態を表す列挙型
/// </summary>
public enum GameStatus
{
    /// <summary>待機中</summary>
    Waiting,
    /// <summary>プレイ中</summary>
    Playing,
    /// <summary>終了</summary>
    Finished
}

/// <summary>
/// しりとりゲームの集約ルートエンティティ
/// </summary>
public sealed class Game : IEquatable<Game>
{
    /// <summary>ゲームID</summary>
    public GameId Id { get; private set; }
    /// <summary>ゲーム状態</summary>
    public GameStatus Status { get; private set; }
    /// <summary>ゲーム設定</summary>
    public GameSettings Settings { get; private set; }
    /// <summary>現在のラウンド</summary>
    public Round CurrentRound { get; private set; }
    /// <summary>参加プレイヤー一覧</summary>
    public IReadOnlyList<Player> Players { get; private set; }
    /// <summary>スコア履歴</summary>
    public IReadOnlyList<ScoreHistory> ScoreHistories { get; private set; }
    /// <summary>作成日時</summary>
    public DateTime CreatedAt { get; private set; }
    /// <summary>更新日時</summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// ゲームの新しいインスタンスを作成します
    /// </summary>
    public Game(GameId id, GameSettings settings,
                GameStatus status, Round currentRound,
                IEnumerable<Player> players, IEnumerable<ScoreHistory> scoreHistories,
                DateTime createdAt, DateTime updatedAt)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        Status = status;
        CurrentRound = currentRound ?? throw new ArgumentNullException(nameof(currentRound));
        Players = players?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(players));
        if (!Players.Any()) throw new ArgumentException("初期プレイヤーが必要です", nameof(players));
        ScoreHistories = scoreHistories?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(scoreHistories));
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    /// <summary>
    /// プレイヤーを追加します
    /// </summary>
    public void AddPlayer(Player player, DateTime now)
    {
        if (Status != GameStatus.Waiting)
            throw new DomainErrorException(DomainErrorCodes.Game.CannotAddPlayerAfterStart, "ゲームが開始されているため、プレイヤーを追加できません");
        if (Players.Count >= Settings.PlayerCount)
            throw new DomainErrorException(DomainErrorCodes.Game.PlayerLimitExceeded, "ゲームが満員です");
        if (Players.Any(p => p.Equals(player)))
            throw new DomainErrorException(DomainErrorCodes.Game.PlayerAlreadyJoined, "このプレイヤーは既に参加しています");

        var updatedPlayers = Players.ToList();
        updatedPlayers.Add(player);
        Players = updatedPlayers.AsReadOnly();
        UpdatedAt = now;
    }

    /// <summary>
    /// ゲームを開始します
    /// </summary>
    public void StartGame(DateTime now)
    {
        if (Status != GameStatus.Waiting)
            throw new DomainErrorException(DomainErrorCodes.Game.AlreadyStarted, "ゲームは既に開始されています");
        if (Players.Count < 2)
            throw new DomainErrorException(DomainErrorCodes.Game.InsufficientPlayers, "ゲームを開始するには最低2人のプレイヤーが必要です");
        if (!Players.All(p => p.IsReady))
            throw new DomainErrorException(DomainErrorCodes.Game.NotAllPlayersReady, "全てのプレイヤーが準備完了状態である必要があります");

        // 最初の描画者を選ぶ
        var firstDrawer = Players.First();
        var updatedPlayers = Players.Select(p =>
            p.Id.Equals(firstDrawer.Id)
                ? new Player(p.Id, p.Name, p.Status, p.IsReady, true)
                : new Player(p.Id, p.Name, p.Status, p.IsReady, false)
        ).ToList();
        Players = updatedPlayers.AsReadOnly();
        
        // 新規Roundを作成（最初の描画者で初期Turnを作成）
        var initialTurn = Turn.CreateInitial(firstDrawer.Id, Settings.TimeLimit, now);
        var newRound = Round.CreateInitial(initialTurn, now);
        CurrentRound = newRound;
        
        Status = GameStatus.Playing;
        UpdatedAt = now;
    }

    /// <summary>
    /// ゲームを終了します
    /// </summary>
    public void EndGame(DateTime now)
    {
        if (Status == GameStatus.Finished)
            throw new DomainErrorException(DomainErrorCodes.Game.AlreadyEnded, "ゲームは既に終了しています");
        Status = GameStatus.Finished;
        UpdatedAt = now;
    }

    /// <summary>
    /// スコア履歴を追加します
    /// </summary>
    public void AddScoreHistory(ScoreHistory scoreHistory, DateTime now)
    {
        var updatedScoreHistories = ScoreHistories.ToList();
        updatedScoreHistories.Add(scoreHistory);
        ScoreHistories = updatedScoreHistories.AsReadOnly();
        UpdatedAt = now;
    }

    /// <summary>
    /// プレイヤーの準備状態を更新します
    /// </summary>
    public void UpdatePlayerReadyStatus(PlayerId playerId, bool isReady, DateTime now)
    {
        var playerIndex = Players.ToList().FindIndex(p => p.Id.Equals(playerId));
        if (playerIndex == -1)
            throw new DomainErrorException(DomainErrorCodes.Game.PlayerNotFound, "プレイヤーが見つかりません");
        var updatedPlayers = Players.ToList();
        var currentPlayer = updatedPlayers[playerIndex];
        var newStatus = isReady ? PlayerStatus.Ready : PlayerStatus.NotReady;
        updatedPlayers[playerIndex] = new Player(currentPlayer.Id, currentPlayer.Name, newStatus, isReady, currentPlayer.IsDrawer);
        Players = updatedPlayers.AsReadOnly();
        UpdatedAt = now;
    }

    /// <summary>
    /// 新規ゲームを作成するファクトリメソッド
    /// </summary>
    /// <param name="id">ゲームID</param>
    /// <param name="settings">ゲーム設定</param>
    /// <param name="initialPlayer">初期プレイヤー</param>
    /// <param name="createdAt">作成日時</param>
    /// <returns>新規作成されたGameインスタンス</returns>
    public static Game CreateNew(GameId id, GameSettings settings, Player initialPlayer, DateTime createdAt)
    {
        var players = new List<Player> { initialPlayer };
        var initialTurn = Turn.CreateInitial(initialPlayer.Id, settings.TimeLimit, createdAt);
        var initialRound = Round.CreateInitial(initialTurn, createdAt);
        return new Game(id, settings, GameStatus.Waiting, initialRound, players, new List<ScoreHistory>(), createdAt, createdAt);
    }

    #region 等価性・演算子
    /// <summary>
    /// 他のゲームと等価かどうかを判定します
    /// </summary>
    public bool Equals(Game? other)
    {
        return other is not null && Id.Equals(other.Id);
    }

    /// <summary>
    /// 他のオブジェクトと等価かどうかを判定します
    /// </summary>
    public override bool Equals(object? obj)
    {
        return Equals(obj as Game);
    }

    /// <summary>
    /// ハッシュコードを取得します
    /// </summary>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <summary>
    /// 等価演算子
    /// </summary>
    public static bool operator ==(Game? left, Game? right)
    {
        return EqualityComparer<Game>.Default.Equals(left, right);
    }

    /// <summary>
    /// 不等価演算子
    /// </summary>
    public static bool operator !=(Game? left, Game? right)
    {
        return !(left == right);
    }
    #endregion
}
