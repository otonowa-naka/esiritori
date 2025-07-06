namespace EsiritoriApi.Tests.Domain.Entities;

using EsiritoriApi.Domain.Game;
using EsiritoriApi.Domain.Game.Entities;
using EsiritoriApi.Domain.Game.ValueObjects;
using EsiritoriApi.Domain.Scoring.ValueObjects;
using EsiritoriApi.Domain.Errors;
using Xunit;

public sealed class GameTests
{
    [Fact]
    public void ゲーム作成時_正常な値でゲームが作成される()
    {
        var gameId = GameId.NewId();
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("テストプレイヤー");
        var creatorId = new PlayerId("player123");
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, DateTime.UtcNow);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), DateTime.UtcNow, DateTime.UtcNow);

        Assert.Equal(gameId, game.Id);
        Assert.Equal(GameStatus.Waiting, game.Status);
        Assert.Equal(settings, game.Settings);
        Assert.Single(game.Players);
        Assert.Equal(creatorName, game.Players.First().Name);
        Assert.Equal(creatorId, game.Players.First().Id);
        Assert.False(game.Players.First().IsReady);
        Assert.False(game.Players.First().IsDrawer);
    }

    [Fact]
    public void プレイヤー追加時_正常にプレイヤーが追加される()
    {
        var now = DateTime.UtcNow;
        var gameId = GameId.NewId();
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = PlayerId.NewId();
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        var newPlayerId = PlayerId.NewId();
        var newPlayerName = new PlayerName("新しいプレイヤー");
        var newPlayer = new Player(newPlayerId, newPlayerName, PlayerStatus.NotReady, false, false);

        game.AddPlayer(newPlayer, now);
        Assert.Equal(2, game.Players.Count);
        Assert.Contains(game.Players, p => p.Id.Equals(newPlayerId));
        Assert.Contains(game.Players, p => p.Name.Equals(newPlayerName));
    }

    [Fact]
    public void プレイヤー追加時_ゲームが満員の場合例外が発生する()
    {
        var now = DateTime.UtcNow;
        var gameId = GameId.NewId();
        var settings = new GameSettings(60, 3, 3); // 最大3人
        var creatorName = new PlayerName("作成者");
        var creatorId = PlayerId.NewId();
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        var player1Id = PlayerId.NewId();
        var player1Name = new PlayerName("プレイヤー1");
        var player1 = new Player(player1Id, player1Name, PlayerStatus.NotReady, false, false);
        game.AddPlayer(player1, now);

        var player2Id = PlayerId.NewId();
        var player2Name = new PlayerName("プレイヤー2");
        var player2 = new Player(player2Id, player2Name, PlayerStatus.NotReady, false, false);
        game.AddPlayer(player2, now);

        // 3人目を追加しようとすると満員で例外発生
        var player3Id = PlayerId.NewId();
        var player3Name = new PlayerName("プレイヤー3");
        var player3 = new Player(player3Id, player3Name, PlayerStatus.NotReady, false, false);

        var exception = Assert.Throws<DomainErrorException>(() => game.AddPlayer(player3, now));
        Assert.Equal(DomainErrorCodes.Game.PlayerLimitExceeded, exception.ErrorCode);
    }

    [Fact]
    public void プレイヤー追加時_既に参加しているプレイヤーの場合例外が発生する()
    {
        var now = DateTime.UtcNow;
        var gameId = GameId.NewId();
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = PlayerId.NewId();
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        var player1Id = PlayerId.NewId();
        var player1Name = new PlayerName("プレイヤー1");
        var player1 = new Player(player1Id, player1Name, PlayerStatus.NotReady, false, false);
        game.AddPlayer(player1, now);
        var exception = Assert.Throws<DomainErrorException>(() => game.AddPlayer(player1, now));
        Assert.Equal(DomainErrorCodes.Game.PlayerAlreadyJoined, exception.ErrorCode);
    }

    [Fact]
    public void プレイヤー追加時_ゲームが開始済みの場合例外が発生する()
    {
        var now = DateTime.UtcNow;
        var gameId = GameId.NewId();
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = PlayerId.NewId();
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        var player1Id = PlayerId.NewId();
        var player1Name = new PlayerName("プレイヤー1");
        var player1 = new Player(player1Id, player1Name, PlayerStatus.NotReady, false, false);
        game.AddPlayer(player1, now);
        game.UpdatePlayerReadyStatus(creatorId, true, now);
        game.UpdatePlayerReadyStatus(player1Id, true, now);
        game.StartGame(now);

        var newPlayerId = PlayerId.NewId();
        var newPlayerName = new PlayerName("新プレイヤー");
        var newPlayer = new Player(newPlayerId, newPlayerName, PlayerStatus.NotReady, false, false);

        var exception = Assert.Throws<DomainErrorException>(() => game.AddPlayer(newPlayer, now));
        Assert.Equal(DomainErrorCodes.Game.CannotAddPlayerAfterStart, exception.ErrorCode);
    }

    [Fact]
    public void ゲーム開始時_正常にゲームが開始される()
    {
        var now = DateTime.UtcNow;
        var gameId = GameId.NewId();
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = PlayerId.NewId();
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        var player1Id = PlayerId.NewId();
        var player1Name = new PlayerName("プレイヤー1");
        var player1 = new Player(player1Id, player1Name, PlayerStatus.NotReady, false, false);
        game.AddPlayer(player1, now);
        game.UpdatePlayerReadyStatus(creatorId, true, now);
        game.UpdatePlayerReadyStatus(player1Id, true, now);
        
        // StartGame前のRoundを保存
        var originalRound = game.CurrentRound;
        
        game.StartGame(now);

        Assert.Equal(GameStatus.Playing, game.Status);
        Assert.Contains(game.Players, p => p.IsDrawer);
        Assert.Single(game.Players.Where(p => p.IsDrawer));
        
        // 新規Roundが作成されていることを確認
        Assert.Equal(1, game.CurrentRound.RoundNumber);
        Assert.Equal(1, game.CurrentRound.CurrentTurn.TurnNumber);
        Assert.Equal(TurnStatus.SettingAnswer, game.CurrentRound.CurrentTurn.Status);
        Assert.Equal(creatorId, game.CurrentRound.CurrentTurn.DrawerId);
        Assert.Equal(settings.TimeLimit, game.CurrentRound.CurrentTurn.TimeLimit);
    }

    [Fact]
    public void ゲーム開始時_プレイヤーが2人未満の場合例外が発生する()
    {
        var now = DateTime.UtcNow;
        var gameId = GameId.NewId();
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = PlayerId.NewId();
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        var exception = Assert.Throws<DomainErrorException>(() => game.StartGame(now));
        Assert.Equal(DomainErrorCodes.Game.InsufficientPlayers, exception.ErrorCode);
    }

    [Fact]
    public void ゲーム開始時_全プレイヤーが準備完了でない場合例外が発生する()
    {
        var now = DateTime.UtcNow;
        var gameId = GameId.NewId();
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = PlayerId.NewId();
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        var player1Id = PlayerId.NewId();
        var player1Name = new PlayerName("プレイヤー1");
        var player1 = new Player(player1Id, player1Name, PlayerStatus.NotReady, false, false);
        game.AddPlayer(player1, now);
        game.UpdatePlayerReadyStatus(creatorId, true, now);
        // player1は準備完了状態にしない

        var exception = Assert.Throws<DomainErrorException>(() => game.StartGame(now));
        Assert.Equal(DomainErrorCodes.Game.NotAllPlayersReady, exception.ErrorCode);
    }

    [Fact]
    public void ゲーム開始時_既に開始されている場合例外が発生する()
    {
        var now = DateTime.UtcNow;
        var gameId = GameId.NewId();
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = PlayerId.NewId();
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Playing, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        var exception = Assert.Throws<DomainErrorException>(() => game.StartGame(now));
        Assert.Equal(DomainErrorCodes.Game.AlreadyStarted, exception.ErrorCode);
    }

    [Fact]
    public void ゲーム終了時_正常にゲームが終了される()
    {
        var now = DateTime.UtcNow;
        var gameId = GameId.NewId();
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = PlayerId.NewId();
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Playing, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        game.EndGame(now);

        Assert.Equal(GameStatus.Finished, game.Status);
    }

    [Fact]
    public void ゲーム終了時_既に終了している場合例外が発生する()
    {
        var now = DateTime.UtcNow;
        var gameId = GameId.NewId();
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = PlayerId.NewId();
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Finished, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        var exception = Assert.Throws<DomainErrorException>(() => game.EndGame(now));
        Assert.Equal(DomainErrorCodes.Game.AlreadyEnded, exception.ErrorCode);
    }

    [Fact]
    public void プレイヤー準備状態更新時_正常に更新される()
    {
        var now = DateTime.UtcNow;
        var gameId = GameId.NewId();
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = PlayerId.NewId();
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        game.UpdatePlayerReadyStatus(creatorId, true, now);

        var updatedPlayer = game.Players.First(p => p.Id.Equals(creatorId));
        Assert.Equal(PlayerStatus.Ready, updatedPlayer.Status);
    }

    [Fact]
    public void プレイヤー準備状態更新時_存在しないプレイヤーの場合例外が発生する()
    {
        var now = DateTime.UtcNow;
        var gameId = GameId.NewId();
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = PlayerId.NewId();
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        var nonExistentPlayerId = PlayerId.NewId();

        var exception = Assert.Throws<DomainErrorException>(() => game.UpdatePlayerReadyStatus(nonExistentPlayerId, true, now));
        Assert.Equal(DomainErrorCodes.Game.PlayerNotFound, exception.ErrorCode);
    }

    [Fact]
    public void スコア履歴追加時_正常に追加される()
    {
        var now = DateTime.UtcNow;
        var gameId = GameId.NewId();
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = PlayerId.NewId();
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Playing, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        var scoreHistory = new ScoreHistory(creator.Id, 1, 1, 10, ScoreReason.CorrectAnswer, now);

        game.AddScoreHistory(scoreHistory, now);

        Assert.Single(game.ScoreHistories);
        Assert.Contains(scoreHistory, game.ScoreHistories);
    }

    [Fact]
    public void CreateNewファクトリメソッド_正常に新規ゲームが作成される()
    {
        var gameId = GameId.NewId();
        var settings = new GameSettings(60, 3, 4);
        var playerName = new PlayerName("初期プレイヤー");
        var playerId = PlayerId.NewId();
        var initialPlayer = new Player(playerId, playerName, PlayerStatus.NotReady, false, false);
        var createdAt = DateTime.UtcNow;

        var game = Game.CreateNew(gameId, settings, initialPlayer, createdAt);

        Assert.Equal(gameId, game.Id);
        Assert.Equal(settings, game.Settings);
        Assert.Equal(GameStatus.Waiting, game.Status);
        Assert.Single(game.Players);
        Assert.Equal(initialPlayer, game.Players.First());
        Assert.Empty(game.ScoreHistories);
        Assert.Equal(createdAt, game.CreatedAt);
        Assert.Equal(createdAt, game.UpdatedAt);
        Assert.NotNull(game.CurrentRound);
        Assert.Equal(1, game.CurrentRound.RoundNumber);
        Assert.Equal(playerId, game.CurrentRound.CurrentTurn.DrawerId);
    }

    [Fact]
    public void コンストラクター_GameIdがnullの場合ArgumentNullExceptionが発生する()
    {
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = PlayerId.NewId();
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, DateTime.UtcNow);
        var now = DateTime.UtcNow;

        var exception = Assert.Throws<ArgumentNullException>(() =>
            new Game(null!, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now));

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void コンストラクター_GameSettingsがnullの場合ArgumentNullExceptionが発生する()
    {
        var gameId = GameId.NewId();
        var creatorName = new PlayerName("作成者");
        var creatorId = PlayerId.NewId();
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, 60, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, DateTime.UtcNow);
        var now = DateTime.UtcNow;

        var exception = Assert.Throws<ArgumentNullException>(() =>
            new Game(gameId, null!, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now));

        Assert.Equal("settings", exception.ParamName);
    }

    [Fact]
    public void コンストラクター_CurrentRoundがnullの場合ArgumentNullExceptionが発生する()
    {
        var gameId = GameId.NewId();
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = PlayerId.NewId();
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var now = DateTime.UtcNow;

        var exception = Assert.Throws<ArgumentNullException>(() =>
            new Game(gameId, settings, GameStatus.Waiting, null!, new[] { creator }, new List<ScoreHistory>(), now, now));

        Assert.Equal("currentRound", exception.ParamName);
    }

    [Fact]
    public void コンストラクター_Playersがnullの場合ArgumentNullExceptionが発生する()
    {
        var gameId = GameId.NewId();
        var settings = new GameSettings(60, 3, 4);
        var creatorId = PlayerId.NewId();
        var initialTurn = Turn.CreateInitial(creatorId, settings.TimeLimit, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, DateTime.UtcNow);
        var now = DateTime.UtcNow;

        var exception = Assert.Throws<ArgumentNullException>(() =>
            new Game(gameId, settings, GameStatus.Waiting, initialRound, null!, new List<ScoreHistory>(), now, now));

        Assert.Equal("players", exception.ParamName);
    }

    [Fact]
    public void コンストラクター_Playersが空の場合ArgumentExceptionが発生する()
    {
        var gameId = GameId.NewId();
        var settings = new GameSettings(60, 3, 4);
        var creatorId = PlayerId.NewId();
        var initialTurn = Turn.CreateInitial(creatorId, settings.TimeLimit, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, DateTime.UtcNow);
        var now = DateTime.UtcNow;

        var exception = Assert.Throws<ArgumentException>(() =>
            new Game(gameId, settings, GameStatus.Waiting, initialRound, new List<Player>(), new List<ScoreHistory>(), now, now));

        Assert.Equal("players", exception.ParamName);
        Assert.Contains("初期プレイヤーが必要です", exception.Message);
    }

    [Fact]
    public void コンストラクター_ScoreHistoriesがnullの場合ArgumentNullExceptionが発生する()
    {
        var gameId = GameId.NewId();
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = PlayerId.NewId();
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, DateTime.UtcNow);
        var now = DateTime.UtcNow;

        var exception = Assert.Throws<ArgumentNullException>(() =>
            new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, null!, now, now));

        Assert.Equal("scoreHistories", exception.ParamName);
    }

    [Fact]
    public void スコア履歴を複数追加_正常に追加される()
    {
        var now = DateTime.UtcNow;
        var gameId = GameId.NewId();
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = PlayerId.NewId();
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Playing, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        var scoreHistory1 = new ScoreHistory(creator.Id, 1, 1, 10, ScoreReason.CorrectAnswer, now);
        var scoreHistory2 = new ScoreHistory(creator.Id, 1, 2, 5, ScoreReason.DrawerPenalty, now.AddMinutes(1));

        game.AddScoreHistory(scoreHistory1, now);
        game.AddScoreHistory(scoreHistory2, now.AddMinutes(1));

        Assert.Equal(2, game.ScoreHistories.Count);
        Assert.Contains(scoreHistory1, game.ScoreHistories);
        Assert.Contains(scoreHistory2, game.ScoreHistories);
        Assert.Equal(now.AddMinutes(1), game.UpdatedAt);
    }

    [Fact]
    public void Equals_同じIDのゲーム同士は等価である()
    {
        var gameId = GameId.NewId();
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = PlayerId.NewId();
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, DateTime.UtcNow);
        var now = DateTime.UtcNow;

        var game1 = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);
        var game2 = new Game(gameId, settings, GameStatus.Playing, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        Assert.True(game1.Equals(game2));
        Assert.True(game1 == game2);
        Assert.False(game1 != game2);
        Assert.Equal(game1.GetHashCode(), game2.GetHashCode());
    }

    [Fact]
    public void Equals_異なるIDのゲーム同士は等価でない()
    {
        var gameId1 = new GameId("123456");
        var gameId2 = new GameId("654321");
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = PlayerId.NewId();
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, DateTime.UtcNow);
        var now = DateTime.UtcNow;

        var game1 = new Game(gameId1, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);
        var game2 = new Game(gameId2, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        Assert.False(game1.Equals(game2));
        Assert.False(game1 == game2);
        Assert.True(game1 != game2);
        Assert.NotEqual(game1.GetHashCode(), game2.GetHashCode());
    }

    [Fact]
    public void Equals_nullとの比較は等価でない()
    {
        var gameId = GameId.NewId();
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = PlayerId.NewId();
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, DateTime.UtcNow);
        var now = DateTime.UtcNow;

        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        Assert.False(game.Equals(null));
        Assert.False(game == null);
        Assert.True(game != null);
    }

    [Fact]
    public void Equals_異なる型のオブジェクトとの比較は等価でない()
    {
        var gameId = GameId.NewId();
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = PlayerId.NewId();
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, DateTime.UtcNow);
        var now = DateTime.UtcNow;

        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);
        var otherObject = "not a game";

        Assert.False(game.Equals(otherObject));
    }

    [Fact]
    public void UpdatedAt更新確認_各操作でUpdatedAtが正しく更新される()
    {
        var now = DateTime.UtcNow;
        var gameId = GameId.NewId();
        var settings = new GameSettings(60, 3, 4);
        var creatorName = new PlayerName("作成者");
        var creatorId = PlayerId.NewId();
        var creator = new Player(creatorId, creatorName, PlayerStatus.NotReady, false, false);
        var initialTurn = Turn.CreateInitial(creator.Id, settings.TimeLimit, DateTime.UtcNow);
        var initialRound = Round.CreateInitial(initialTurn, now);
        var game = new Game(gameId, settings, GameStatus.Waiting, initialRound, new[] { creator }, new List<ScoreHistory>(), now, now);

        // プレイヤー追加
        var newPlayerId = PlayerId.NewId();
        var newPlayerName = new PlayerName("新プレイヤー");
        var newPlayer = new Player(newPlayerId, newPlayerName, PlayerStatus.NotReady, false, false);
        var addPlayerTime = now.AddMinutes(1);
        game.AddPlayer(newPlayer, addPlayerTime);
        Assert.Equal(addPlayerTime, game.UpdatedAt);

        // 準備状態更新
        var readyUpdateTime = now.AddMinutes(2);
        game.UpdatePlayerReadyStatus(creatorId, true, readyUpdateTime);
        Assert.Equal(readyUpdateTime, game.UpdatedAt);

        // ゲーム開始
        game.UpdatePlayerReadyStatus(newPlayerId, true, readyUpdateTime);
        var startTime = now.AddMinutes(3);
        game.StartGame(startTime);
        Assert.Equal(startTime, game.UpdatedAt);

        // スコア追加
        var scoreHistory = new ScoreHistory(creatorId, 1, 1, 10, ScoreReason.CorrectAnswer, now);
        var scoreAddTime = now.AddMinutes(4);
        game.AddScoreHistory(scoreHistory, scoreAddTime);
        Assert.Equal(scoreAddTime, game.UpdatedAt);

        // ゲーム終了
        var endTime = now.AddMinutes(5);
        game.EndGame(endTime);
        Assert.Equal(endTime, game.UpdatedAt);
    }
}
