namespace EsiritoriApi.Domain.Entities;

using EsiritoriApi.Domain.ValueObjects;
using EsiritoriApi.Domain.Entities;

public enum GameStatus
{
    Waiting,
    Playing,
    Finished
}

public sealed class Game : IEquatable<Game>
{
    public GameId Id { get; }
    public GameStatus Status { get; }
    public GameSettings Settings { get; }
    public Round CurrentRound { get; }
    public IReadOnlyList<Player> Players { get; }
    public IReadOnlyList<ScoreHistory> ScoreHistories { get; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }

    public Game(GameId id, GameSettings settings, PlayerName creatorName, PlayerId creatorId,
                GameStatus status = GameStatus.Waiting, Round? currentRound = null,
                IEnumerable<Player>? players = null, IEnumerable<ScoreHistory>? scoreHistories = null,
                DateTime? createdAt = null, DateTime? updatedAt = null)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        Status = status;
        ScoreHistories = scoreHistories?.ToList().AsReadOnly() ?? new List<ScoreHistory>().AsReadOnly();
        CreatedAt = createdAt ?? DateTime.UtcNow;
        UpdatedAt = updatedAt ?? DateTime.UtcNow;

        if (players != null)
        {
            Players = players.ToList().AsReadOnly();
        }
        else
        {
            var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
            Players = new List<Player> { creator }.AsReadOnly();
        }

        if (currentRound != null)
        {
            CurrentRound = currentRound;
        }
        else
        {
            var initialTurn = Turn.CreateInitial(creatorId, settings.TimeLimit);
            CurrentRound = Round.CreateNew(initialTurn, DateTime.UtcNow);
        }
    }

    public Game AddPlayer(PlayerId playerId, PlayerName playerName)
    {
        if (Status != GameStatus.Waiting)
        {
            throw new InvalidOperationException("ゲームが開始されているため、プレイヤーを追加できません");
        }

        if (Players.Count >= Settings.PlayerCount)
        {
            throw new InvalidOperationException("ゲームが満員です");
        }

        if (Players.Any(p => p.Id.Equals(playerId)))
        {
            throw new InvalidOperationException("このプレイヤーは既に参加しています");
        }

        var newPlayer = new Player(playerId, playerName, PlayerStatus.NotReady, false, false);
        var updatedPlayers = Players.ToList();
        updatedPlayers.Add(newPlayer);

        return new Game(
            Id,
            Settings,
            Players.First().Name,
            Players.First().Id,
            Status,
            CurrentRound,
            updatedPlayers,
            ScoreHistories,
            CreatedAt,
            DateTime.UtcNow
        );
    }

    public Game StartGame()
    {
        if (Status != GameStatus.Waiting)
        {
            throw new InvalidOperationException("ゲームは既に開始されています");
        }

        if (Players.Count < 2)
        {
            throw new InvalidOperationException("ゲームを開始するには最低2人のプレイヤーが必要です");
        }

        if (!Players.All(p => p.IsReady))
        {
            throw new InvalidOperationException("全てのプレイヤーが準備完了状態である必要があります");
        }

        var firstDrawer = Players.First();
        var updatedTurn = CurrentRound.CurrentTurn
            .WithStatus(TurnStatus.SettingAnswer);

        var updatedRound = CurrentRound
            .WithTurn(updatedTurn)
            .WithStartTime(DateTime.UtcNow);

        var updatedPlayers = Players.Select(p =>
            p.Id.Equals(firstDrawer.Id) 
                ? new Player(p.Id, p.Name, p.Status, p.IsReady, true)
                : new Player(p.Id, p.Name, p.Status, p.IsReady, false)
        ).ToList();

        return new Game(
            Id,
            Settings,
            Players.First().Name,
            Players.First().Id,
            GameStatus.Playing,
            updatedRound,
            updatedPlayers,
            ScoreHistories,
            CreatedAt,
            DateTime.UtcNow
        );
    }

    public Game EndGame()
    {
        if (Status == GameStatus.Finished)
        {
            throw new InvalidOperationException("ゲームは既に終了しています");
        }

        var updatedRound = CurrentRound.WithEndTime(DateTime.UtcNow);

        return new Game(
            Id,
            Settings,
            Players.First().Name,
            Players.First().Id,
            GameStatus.Finished,
            updatedRound,
            Players,
            ScoreHistories,
            CreatedAt,
            DateTime.UtcNow
        );
    }

    public Game AddScoreHistory(ScoreHistory scoreHistory)
    {
        var updatedScoreHistories = ScoreHistories.ToList();
        updatedScoreHistories.Add(scoreHistory);

        return new Game(
            Id,
            Settings,
            Players.First().Name,
            Players.First().Id,
            Status,
            CurrentRound,
            Players,
            updatedScoreHistories,
            CreatedAt,
            DateTime.UtcNow
        );
    }

    public Game UpdatePlayerReadyStatus(PlayerId playerId, bool isReady)
    {
        var playerIndex = Players.ToList().FindIndex(p => p.Id.Equals(playerId));
        if (playerIndex == -1)
        {
            throw new InvalidOperationException("プレイヤーが見つかりません");
        }

        var updatedPlayers = Players.ToList();
        var currentPlayer = updatedPlayers[playerIndex];
        updatedPlayers[playerIndex] = new Player(currentPlayer.Id, currentPlayer.Name, currentPlayer.Status, isReady, currentPlayer.IsDrawer);

        return new Game(
            Id,
            Settings,
            Players.First().Name,
            Players.First().Id,
            Status,
            CurrentRound,
            updatedPlayers,
            ScoreHistories,
            CreatedAt,
            DateTime.UtcNow
        );
    }



    public bool Equals(Game? other)
    {
        return other is not null && Id.Equals(other.Id);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Game);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Game? left, Game? right)
    {
        return EqualityComparer<Game>.Default.Equals(left, right);
    }

    public static bool operator !=(Game? left, Game? right)
    {
        return !(left == right);
    }
}
